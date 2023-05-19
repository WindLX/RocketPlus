using RocketPlus.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RocketPlus.Models
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public static Vector3 Zero => new() { X = 0, Y = 0, Z = 0 };

        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !v1.Equals(v2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            var other = (Vector3)obj;
            if (other.X == X && other.Y == Y && other.Z == Z)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public struct Position
    {
        public float Lat { get; set; }
        public float Lon { get; set; }
        public static Position Zero => new() { Lat = 0, Lon = 0 };

        public override string ToString()
        {
            return $"{Lat},{Lon}";
        }

        public static bool operator ==(Position p1, Position p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Position p1, Position p2)
        {
            return !p1.Equals(p2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            var other = (Position)obj;
            if (other.Lat == Lat && other.Lon == Lon)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public class MessageDataModel
    {
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Altitude { get; set; }
        public Vector3 Acc { get; set; }
        public Vector3 AnguSpeed { get; set; }
        public Vector3 Posture { get; set; }
        public static string NameString => "Temperature,Pressure,Altitude,AccX,AccY,AccZ,AnguSpeedX,AnguSpeedY,AnguSpeedZ,Roll,Pitch,Yaw";
        public static MessageDataModel Zero => new()
        {
            Temperature = 0.0F,
            Altitude = 0.0F,
            Pressure = 0.0F,
            Acc = new Vector3 { X = 0.0F, Y = 0.0F, Z = 0.0F },
            AnguSpeed = new Vector3 { X = 0.0F, Y = 0.0F, Z = 0.0F },
            Posture = new Vector3 { X = 0.0F, Y = 0.0F, Z = 0.0F },
        };

        public override string ToString()
        {
            return $"{Temperature},{Pressure},{Altitude},{Acc},{AnguSpeed},{Posture}";
        }

        public float this[int i]
        {
            get
            {
                return i switch
                {
                    0 => Temperature,
                    1 => Pressure,
                    2 => Altitude,
                    3 => Acc.X,
                    4 => Acc.Y,
                    5 => Acc.Z,
                    6 => AnguSpeed.X,
                    7 => AnguSpeed.Y,
                    8 => AnguSpeed.Z,
                    9 => Posture.X,
                    10 => Posture.Y,
                    11 => Posture.Z,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
        }

        public static bool operator ==(MessageDataModel m1, MessageDataModel m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(MessageDataModel m1, MessageDataModel m2)
        {
            return !m1.Equals(m2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            bool res = true;
            var other = obj as MessageDataModel;
            for (int i = 0; i < 12; i++)
                if (this[i] != other?[i])
                    res &= false;
            return res;
        }

        public override int GetHashCode()
        {
            return ToString().GetType().GetHashCode();
        }
    }

    public class MapDataModel
    {
        public Position Position { get; set; }
        public DateTime DateTime { get; set; }
        public static MapDataModel Zero => new()
        {
            Position = new Position() { Lat = 0.0F, Lon = 0.0F, },
            DateTime = new DateTime(),
        };
    }

    public static class MessageDataConverter
    {
        public static MessageDataModel RawMessageToDataConverter(string msg)
        {
            var msgDict = GetMsgDict(msg);
            if (IsMsgValid(msgDict))
            {
                try
                {
                    var RocketData = new MessageDataModel()
                    {
                        Temperature = float.Parse(msgDict["Temperature"]),
                        Altitude = float.Parse(msgDict["Altitude"]),
                        Pressure = float.Parse(msgDict["Pressure"]),
                        Acc = new Vector3 { X = float.Parse(msgDict["AccX"]), Y = float.Parse(msgDict["AccY"]), Z = float.Parse(msgDict["AccZ"]) },
                        AnguSpeed = new Vector3 { X = float.Parse(msgDict["AnguSpeX"]), Y = float.Parse(msgDict["AnguSpeY"]), Z = float.Parse(msgDict["AnguSpeZ"]) },
                        Posture = new Vector3 { X = float.Parse(msgDict["RollAngle"]), Y = float.Parse(msgDict["PitchAngle"]), Z = float.Parse(msgDict["YawAngle"]) },
                    };
                    return RocketData;
                }
                catch
                {
                    return MessageDataModel.Zero;
                }
            }
            return MessageDataModel.Zero;
        }

        public static MapDataModel RawMessageToMapConverter(string msg)
        {
            var msgDict = GetMsgDict(msg);
            if (IsMsgValid(msgDict))
            {
                try
                {
                    DateTime dt = DateTime.ParseExact(msgDict["UTC"], "HHmmss.fff", null);
                    dt = dt.AddHours(8);

                    var MapData = new MapDataModel()
                    {
                        DateTime = dt,
                        Position = new Position
                        {
                            Lat = float.Parse(msgDict["Lat"][1..3]) + float.Parse(msgDict["Lat"][3..]) / 60,
                            Lon = float.Parse(msgDict["Lon"][1..4]) + float.Parse(msgDict["Lon"][4..]) / 60,
                        }
                    };
                    return MapData;
                }
                catch
                {
                    return MapDataModel.Zero;
                }
            }
            return MapDataModel.Zero;
        }

        public static Vector3 RawMessageToPostureConverter(string msg)
        {
            var msgDict = GetMsgDict(msg);
            if (IsMsgValid(msgDict))
            {
                try
                {
                    var posture = new Vector3()
                    {
                        X = float.Parse(msgDict["RollAngle"]),
                        Y = float.Parse(msgDict["PitchAngle"]),
                        Z = float.Parse(msgDict["YawAngle"]),
                    };
                    return posture;
                }
                catch
                {
                    return Vector3.Zero;
                }
            }
            return Vector3.Zero;
        }

        private static Dictionary<string, string> GetMsgDict(string msg)
        {
            try
            {
                var strList = msg.TrimEnd().Split('&');
                string targetStr = "";
                foreach (var str in strList)
                {
                    if (str.StartsWith("msg=") && !str.StartsWith("msg=OPEN"))
                        targetStr = str[4..];
                }
                if (targetStr != "")
                {
                    var msgList = targetStr.Split(",");
                    Dictionary<string, string> msgDict = new();
                    foreach (var item in msgList)
                    {
                        try
                        {
                            (string key, string value) = (item.Split('=')[0], item.Split('=')[1]);
                            if (msgDict.ContainsKey(key))
                            {
                                msgDict[key] = value;
                            }
                            else
                            {
                                msgDict.Add(key, value);
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return new Dictionary<string, string>();
                        }
                    }
                    return msgDict;
                }
                return new Dictionary<string, string>();
            }
            catch { return new Dictionary<string, string>(); }
        }

        private static bool IsMsgValid(Dictionary<string, string> msgDict)
        {
            string[] keys = { "Temperature", "Altitude", "Pressure", "AccX", "AccY", "AccZ", "AnguSpeX", "AnguSpeY", "AnguSpeZ", "RollAngle", "YawAngle", "PitchAngle", "UTC", "Lat", "Lon" };
            bool res = true;
            foreach (var key in keys)
            {
                if (!msgDict.ContainsKey(key))
                    res &= false;
            }
            return res;
        }

        public static List<MessageDataModel>? FileToDataConverter(string filePath)
        {
            if (File.Exists(filePath))
            {
                List<MessageDataModel> Data = new();
                string? line;
                if (Path.GetExtension(filePath) == ".txt")
                {
                    using StreamReader sr = new(filePath);
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
                else if (Path.GetExtension(filePath) == ".csv")
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    Encoding gb2312 = Encoding.GetEncoding("GB2312");
                    using StreamReader sr = new(filePath, gb2312);
                    var key = sr.ReadLine();
                    if (key != null)
                    {
                        List<string> keys = new();
                        if (key.StartsWith("#"))
                        {
                            key = key[1..];
                            keys = key.Trim().TrimEnd().Split(',').Select(s =>
                            {
                                return s switch
                                {
                                    "高度 (m)" => "Altitude",
                                    "空气温度 (°C)" => "Temperature",
                                    "空气压力 (mbar)" => "Pressure",
                                    "滚转角速度 (°/s)" => "AnguSpeedZ",
                                    "俯仰角速度 (°/s)" => "AnguSpeedY",
                                    "偏航角速度 (°/s)" => "AnguSpeedX",
                                    "垂直加速度 (m/s?)" => "垂直加速度 (m/s?)",
                                    "横向加速度 (m/s?)" => "横向加速度 (m/s?)",
                                    "垂直方向 (天顶角) (°)" => "垂直方向 (天顶角) (°)",
                                    "水平方向 (方位角) (°)" => "水平方向 (方位角) (°)",
                                    _ => "",
                                };
                            }).ToList();
                        }
                        else
                        {
                            keys = key
                             .Trim()
                             .TrimEnd()
                             .Split(",")
                             .ToList();
                        }

                        while ((line = sr.ReadLine()) != null)
                        {
                            Dictionary<string, string> msgDict = line
                                .Trim()
                                .TrimEnd()
                                .Split(',')
                                .Select((value, index) => new { Index = index, Value = value })
                                .Where(pair => keys[pair.Index] != "")
                                .ToDictionary(pair => keys[pair.Index], pair => pair.Value);
                            float temperature = float.Parse(msgDict["Temperature"]);
                            float altitude = float.Parse(msgDict["Altitude"]);
                            float pressure = float.Parse(msgDict["Pressure"]);
                            Vector3 anguSpeed = new() { X = float.Parse(msgDict["AnguSpeedX"]), Y = float.Parse(msgDict["AnguSpeedY"]), Z = float.Parse(msgDict["AnguSpeedZ"]) };
                            Vector3 acc = new();
                            Vector3 posture = new();
                            if (msgDict.ContainsKey("AccX"))
                            {
                                acc = new() { X = float.Parse(msgDict["AccX"]), Y = float.Parse(msgDict["AccY"]), Z = float.Parse(msgDict["AccZ"]) };
                                posture = new() { X = float.Parse(msgDict["Roll"]), Y = float.Parse(msgDict["Pitch"]), Z = float.Parse(msgDict["Yaw"]) };
                            }
                            else
                            {
                                float verticalAcceleration = float.Parse(msgDict["垂直加速度 (m/s?)"]);
                                float lateralAcceleration = float.Parse(msgDict["横向加速度 (m/s?)"]);
                                float verticalAngle = float.Parse(msgDict["垂直方向 (天顶角) (°)"]);
                                float horizontalAngle = float.Parse(msgDict["水平方向 (方位角) (°)"]);

                                // 计算XYZ方向的加速度
                                double xAcceleration = lateralAcceleration * Math.Cos(horizontalAngle) - verticalAcceleration * Math.Sin(horizontalAngle) * Math.Sin(verticalAngle);
                                double yAcceleration = -lateralAcceleration * Math.Sin(horizontalAngle) - verticalAcceleration * Math.Cos(horizontalAngle) * Math.Sin(verticalAngle);
                                double zAcceleration = verticalAcceleration * Math.Cos(verticalAngle);

                                // 计算姿态角
                                double rollAngle = Math.Atan2(-yAcceleration, zAcceleration) * 180 / Math.PI;
                                double pitchAngle = Math.Atan2(xAcceleration, Math.Sqrt(yAcceleration * yAcceleration + zAcceleration * zAcceleration)) * 180 / Math.PI;
                                double yawAngle = Math.Atan2(-lateralAcceleration, Math.Sqrt(verticalAcceleration * verticalAcceleration + lateralAcceleration * lateralAcceleration)) * 180 / Math.PI;

                                acc = new() { X = (float)xAcceleration, Y = (float)yAcceleration, Z = (float)zAcceleration };
                                posture = new() { X = (float)rollAngle, Y = (float)pitchAngle, Z = (float)yawAngle };
                            }
                            Data.Add(new MessageDataModel()
                            {
                                Temperature = temperature,
                                Altitude = altitude,
                                Pressure = pressure,
                                Acc = acc,
                                AnguSpeed = anguSpeed,
                                Posture = posture,
                            });
                        }
                    }
                }
                return Data;
            }
            else return null;
        }
    }
}
