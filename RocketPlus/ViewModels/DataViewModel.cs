using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RocketPlus.Models;
using RocketPlus.Services;
using RocketPlus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RocketPlus.ViewModels
{
    public partial class DataViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MessageDataModel> data = new();

        public DataViewModel()
        {
            Client.Instace.OnRawMessage += (msg) =>
            {
                if (MessageDataConverter.RawMessageToDataConverter(msg) != MessageDataModel.Zero)
                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        Data.Add(MessageDataConverter.RawMessageToDataConverter(msg));
                    });
            };

            WeakReferenceMessenger.Default.Register<FileDataMessage>(this, ReadData);
        }

        private void ReadData(object recipient, FileDataMessage message)
        {
            if (message.MessageData != null)
            {
                Data = new();
                message.MessageData.ForEach(s => Data.Add(s));
            }
        }

        [RelayCommand]
        private void ExportData()
        {
            try
            {
                StreamWriter streamWriter = new("./Data.csv", false, Encoding.UTF8);
                streamWriter.WriteLine(MessageDataModel.NameString);
                foreach (var item in Data)
                {
                    streamWriter.WriteLine(item);
                }
                streamWriter.Close();
                streamWriter.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [RelayCommand]
        private void ClearData()
        {
            Data.Clear();
        }

        [RelayCommand]
        private void ReadData()
        {
            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                Title = "请选择数据文件",
                DefaultExt = ".csv",
                Filter = @"数据文件(*.csv,*.txt,*.TXT)|*.csv;*.txt;*.TXT"
            };
            bool? res = dialog.ShowDialog();
            if (res == true)
            {
                var file = dialog.FileName;
                List<MessageDataModel>? data = MessageDataConverter.FileToDataConverter(file);
                WeakReferenceMessenger.Default.Send(new FileDataMessage(data));
            }
        }

        [RelayCommand]
        private void SendData()
        {
            Thread sendThread = new(SendDataFunc) { IsBackground = true };
            sendThread.Start();
        }

        private void SendDataFunc()
        {
            Data.ToList().ForEach(data =>
            {
                UnityClient.Instace.Send(data.ToString());
                Thread.Sleep(200);
            });
        }
    }
}
