using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using RocketPlus.Models;
using RocketPlus.Services;
using RocketPlus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;

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

            WeakReferenceMessenger.Default.Register<FilePathMessage>(this, ReadData);
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

        private void ReadData(object recipient, FilePathMessage path)
        {
            if (File.Exists(path.filePath))
            {
                data = new();
                using (StreamReader sr = new StreamReader(path.filePath))
                {
                    string? line;
                    if (Path.GetExtension(path.filePath) == ".txt")
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            Dictionary<string, string> msgDict = line
                                .Trim()
                                .TrimEnd()
                                .Split(',')
                                .Select(pair => new KeyValuePair<string, string>(pair.Split("=")[0], pair.Split("=")[1]))
                                .Where((value, index) => index > 0 && index < 13)
                                .ToDictionary(pair => pair.Key, pair => pair.Value);
                            data.Add(new MessageDataModel()
                            {
                                Temperature = float.Parse(msgDict["Temperature"]),
                                Altitude = float.Parse(msgDict["Altitude"]),
                                Pressure = float.Parse(msgDict["Pressure"]),
                                Acc = new Vector3 { X = float.Parse(msgDict["AccX"]), Y = float.Parse(msgDict["AccY"]), Z = float.Parse(msgDict["AccZ"]) },
                                AnguSpeed = new Vector3 { X = float.Parse(msgDict["AnguSpeX"]), Y = float.Parse(msgDict["AnguSpeY"]), Z = float.Parse(msgDict["AnguSpeZ"]) },
                                Posture = new Vector3 { X = float.Parse(msgDict["RollAngle"]), Y = float.Parse(msgDict["PitchAngle"]), Z = float.Parse(msgDict["YawAngle"]) },
                            });
                        }
                    }
                    else if (Path.GetExtension(path.filePath) == ".csv")
                    {
                        var key = sr.ReadLine();
                        List<string> keys = key
                            .Trim()
                            .TrimEnd()
                            .Split(",")
                            .ToList();
                        while ((line = sr.ReadLine()) != null)
                        {
                            Dictionary<string, string> msgDict = line
                                .Trim()
                                .TrimEnd()
                                .Split(',')
                                .Select((value, index) => new KeyValuePair<string, string>(keys[index], value))
                                .ToDictionary(pair => pair.Key, pair => pair.Value);
                            data.Add(new MessageDataModel()
                            {
                                Temperature = float.Parse(msgDict["Temperature"]),
                                Altitude = float.Parse(msgDict["Altitude"]),
                                Pressure = float.Parse(msgDict["Pressure"]),
                                Acc = new Vector3 { X = float.Parse(msgDict["AccX"]), Y = float.Parse(msgDict["AccY"]), Z = float.Parse(msgDict["AccZ"]) },
                                AnguSpeed = new Vector3 { X = float.Parse(msgDict["AnguSpeedX"]), Y = float.Parse(msgDict["AnguSpeedY"]), Z = float.Parse(msgDict["AnguSpeedZ"]) },
                                Posture = new Vector3 { X = float.Parse(msgDict["Roll"]), Y = float.Parse(msgDict["Pitch"]), Z = float.Parse(msgDict["Yaw"]) },
                            });
                        }
                    }
                }
                UpdateDataAll();
            }
        }
    }
}
