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

            WeakReferenceMessenger.Default.Register<FilePathMessage>(this, ReadData);
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
                WeakReferenceMessenger.Default.Send(new FilePathMessage(file));
            }
        }

        private void ReadData(object recipient, FilePathMessage path)
        {
            if (File.Exists(path.filePath))
            {
                Data = new();
                using StreamReader sr = new(path.filePath);
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
                        Data.Add(new MessageDataModel()
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
                        Data.Add(new MessageDataModel()
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
        }
    }
}
