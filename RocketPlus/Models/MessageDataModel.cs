using RocketPlus.Utils;
using System;
using System.Collections.Generic;

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
                    Random ra = new();
                    var MapData = new MapDataModel()
                    {
                        DateTime = DateTime.Now,
                        Position = new Position
                        {
                            Lat = ra.NextFloat(103.930759F, 103.931687F),
                            Lon = ra.NextFloat(30.742709F, 30.744032F),
                        }
                    };
                    //var MapData = new MapDataModel()
                    //{
                    //    DateTime = DateTime.Now,
                    //    Position = new Position
                    //    {
                    //        Lat = float.Parse(msgDict["Lat"][..^7]) + float.Parse(msgDict["Lat"][^8..]) / 60,
                    //        Lon = float.Parse(msgDict["Lon"][..^7]) + float.Parse(msgDict["Lon"][^8..]) / 60,
                    //    }
                    //};
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
    }
}
