using System;
using System.IO;
using System.Xml.Serialization;

namespace TurboPort
{

    /// <summary>
    /// Summary description for Settings.
    /// </summary>
    public class Settings
    {
        static public Settings Current = new Settings();
        static public readonly string CurrentProfileName = "Config.xml";
        public void Save(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stream, this, new XmlSerializerNamespaces());
        }

        public static Settings LoadCurrentProfile()
        {
            try
            {
                FileStream fs = new FileStream(Path.Combine(DirectorySettings.ConfigDir, CurrentProfileName), FileMode.Open, FileAccess.Read);
                Settings result = Load(fs);
                fs.Close();
                return result;
            }
            catch(FileNotFoundException)
            {
                return new Settings();
            }
        }
        public static Settings Load(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            Settings settings = (Settings)serializer.Deserialize(stream);

            return settings;
        }

        public enum LevelType
        {
            Gp2,
        }
        public string FileName;
        public PlayerConfig[] Players = new PlayerConfig[] { new PlayerConfig(), new PlayerConfig()  };
    }

    public class AnalogControlAttribute : Attribute
    {
    }

    public class Controls
    {
        [AnalogControl]
        public string Thrust;
        public string ThrustKey;
        [AnalogControl]
        public string RotateLeftRight;
        public string RotateLeft;
        public string RotateRight;
        public string Fire;
        public string FireSpecial;
    }

    public class PlayerConfig
    {
        public string Name    = "Unnamed";
        public string Controller;
        public Controls Controls = new Controls();
    }



    public class DirectorySettings
    {
        public static string MediaDir
        {
            get
            {
                return @"D:\src\MyProjects\tglrf.xna\tglrf.xna\media";
            }
        }

        public static string ConfigDir
        {
            get { return @"D:\src\MyProjects\tglrf.xna"; }
        }
    }
}
