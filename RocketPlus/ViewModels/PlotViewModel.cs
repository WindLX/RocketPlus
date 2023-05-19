using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using RocketPlus.Models;
using RocketPlus.Services;
using RocketPlus.Utils;
using RocketPlus.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RocketPlus.ViewModels
{
    public partial class PlotViewModel : ObservableObject
    {
        private List<LineSeries> lineSeries = new();
        private List<bool> showList = new();

        [ObservableProperty]
        private ObservableCollection<bool> isCheckAble = new() { true, false, false, false };

        [ObservableProperty]
        private ObservableCollection<bool> checkList = new();

        [ObservableProperty]
        private PlotModel curData = new();

        private List<MessageDataModel> data = new();

        public PlotViewModel()
        {
            checkList.Add(true);
            for (int i = 0; i < 11; i++)
                checkList.Add(false);

            UpdateDataAll();

            Client.Instace.OnRawMessage += (msg) =>
            {
                if (MessageDataConverter.RawMessageToDataConverter(msg) != MessageDataModel.Zero)
                {
                    data.Add(MessageDataConverter.RawMessageToDataConverter(msg));
                    UpdateData();
                    CurData.InvalidatePlot(true);
                }
            };

            WeakReferenceMessenger.Default.Register<FileDataMessage>(this, ReadData);
        }

        [RelayCommand]
        private void CheckGroup(object group)
        {
            var isCheckAbleTemp = new ObservableCollection<bool>() { false, false, false, false };
            isCheckAbleTemp[Convert.ToInt32(group)] = true;
            IsCheckAble = isCheckAbleTemp;

            bool fi = CheckList
                .Where((value, index) => index >= Convert.ToInt32(group) * 3 && index < (Convert.ToInt32(group) + 1) * 3)
                .ToList()
                .All(value => value == false);
            if (fi)
                CheckList[Convert.ToInt32(group) * 3] = true;

            UpdateDataAll();
        }

        [RelayCommand]
        private void UpdateCheck(object check) => UpdateDataAll();

        private void PlotInit()
        {
            CurData.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            CurData.PlotAreaBorderColor = OxyColor.Parse("#9C9C9C");
            CurData.TextColor = OxyColor.Parse("#1E436B");
            CurData.DefaultFontSize = 14;
            LinearAxis linearAxis1 = new() { Position = AxisPosition.Left };
            LinearAxis linearAxis2 = new() { Position = AxisPosition.Bottom };
            CurData.Axes.Add(linearAxis1);
            CurData.Axes.Add(linearAxis2);
        }

        private void UpdateDataAll()
        {
            UpdateShowList();
            CurData = new PlotModel();
            PlotInit();

            lineSeries = new();
            for (int i = 0; i < 12; i++)
            {
                if (showList[i])
                {
                    LineSeries lineSeries1 = new()
                    {
                        StrokeThickness = 1,
                        CanTrackerInterpolatePoints = true,
                        InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    };
                    for (int k = 0; k < data.Count; k++)
                        lineSeries1.Points.Add(new DataPoint(k, data[k][i]));
                    lineSeries.Add(lineSeries1);
                }
            }

            foreach (var item in lineSeries)
            {
                CurData.Series.Add(item);
            }

            CurData.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendTextColor = OxyColors.LightGray
            });
            CurData.InvalidatePlot(true);
        }

        private void UpdateData()
        {
            int j = 0;
            for (int i = 0; i < 12; i++)
            {
                if (showList[i])
                {
                    lineSeries[j].Points.Clear();
                    for (int k = 0; k < data.Count; k++)
                        lineSeries[j].Points.Add(new DataPoint(k, data[k][i]));
                    j++;
                }
            }
        }

        private void UpdateShowList()
        {
            showList = new List<bool>();
            for (int i = 0; i < 12; i++)
                showList.Add(IsCheckAble[i / 3] && CheckList[i]);
        }

        private void ReadData(object recipient, FileDataMessage message)
        {
            if (message.MessageData != null)
            {
                data = new();
                message.MessageData.ForEach(s => data.Add(s));
                UpdateDataAll();
            }
        }

    }
}
