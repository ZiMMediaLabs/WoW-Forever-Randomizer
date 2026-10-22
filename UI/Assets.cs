using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Reflection;

namespace WoWForeverRandomizer.UI
{
    // Loads the images, fonts and sounds embedded in the .exe.
    public static class Assets
    {
        private const string ResourcePrefix = "WoWForeverRandomizer.Resources.";

        private static readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, SoundPlayer> _soundCache = new Dictionary<string, SoundPlayer>();
        private static readonly List<PrivateFontCollection> _fontCollections = new List<PrivateFontCollection>();

        private static FontFamily _titleFamily;
        private static FontFamily _bodyFamily;

        public static FontFamily TitleFont
        {
            get { return _titleFamily ?? (_titleFamily = LoadFont("Fonts/CinzelDecorative-Bold.ttf", "Cinzel Decorative WoW")); }
        }

        public static FontFamily BodyFont
        {
            get { return _bodyFamily ?? (_bodyFamily = LoadFont("Fonts/Cinzel-Bold.ttf", "Cinzel WoW")); }
        }

        public static Image GetImage(string path)
        {
            Image image;
            if (_imageCache.TryGetValue(path, out image))
            {
                return image;
            }

            using (Stream stream = OpenResource(path))
            {
                // Copy into a Bitmap so the stream can be closed safely.
                using (Image temp = Image.FromStream(stream))
                {
                    image = new Bitmap(temp);
                }
            }

            _imageCache[path] = image;
            return image;
        }

        private static Icon _appIcon;

        public static Icon GetAppIcon()
        {
            if (_appIcon == null)
            {
                using (Stream stream = OpenResource("Icons/wow_icon.ico"))
                {
                    _appIcon = new Icon(stream, 32, 32);
                }
            }

            return _appIcon;
        }

        // Plays a .wav from a "Sounds" folder next to the .exe if one exists (custom sounds),
        // otherwise the embedded copy. Errors are ignored.
        public static void PlaySound(string fileName)
        {
            try
            {
                SoundPlayer player;
                if (!_soundCache.TryGetValue(fileName, out player))
                {
                    Stream sound = OpenSound(fileName);
                    if (sound != null)
                    {
                        player = new SoundPlayer(sound);
                        player.Load();
                    }

                    _soundCache[fileName] = player;
                }

                if (player == null)
                {
                    return;
                }

                player.Stream.Position = 0;
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing audio: " + ex.Message);
            }
        }

        private static Stream OpenSound(string fileName)
        {
            string overridePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", fileName);
            if (File.Exists(overridePath))
            {
                return new MemoryStream(File.ReadAllBytes(overridePath));
            }

            string resourceName = ResourcePrefix + "Audio." + fileName;
            Stream embedded = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (embedded == null)
            {
                return null;
            }

            var buffer = new MemoryStream();
            using (embedded)
            {
                embedded.CopyTo(buffer);
            }

            buffer.Position = 0;
            return buffer;
        }

        public static FontStyle StyleFor(FontFamily family)
        {
            foreach (FontStyle style in new[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic })
            {
                if (family.IsStyleAvailable(style))
                {
                    return style;
                }
            }

            return FontStyle.Regular;
        }

        private static FontFamily LoadFont(string path, string familyName)
        {
            if (IsFontInstalled(familyName))
            {
                return new FontFamily(familyName);
            }

            try
            {
                byte[] fontData;
                using (Stream stream = OpenResource(path))
                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    fontData = memory.ToArray();
                }

                // GDI+ is more reliable with AddFontFile than AddMemoryFont, so load from a temp copy.
                string fontFolder = Path.Combine(Path.GetTempPath(), "WoWForeverRandomizer");
                Directory.CreateDirectory(fontFolder);
                string fontFile = Path.Combine(fontFolder, Path.GetFileName(path));
                try
                {
                    File.WriteAllBytes(fontFile, fontData);
                }
                catch (IOException)
                {
                    // Already in use by another instance; the file is identical.
                }

                var collection = new PrivateFontCollection();
                collection.AddFontFile(fontFile);
                _fontCollections.Add(collection);
                return collection.Families[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Font load failed, using fallback: " + ex.Message);
                return new FontFamily("Georgia");
            }
        }

        private static bool IsFontInstalled(string familyName)
        {
            using (var installed = new InstalledFontCollection())
            {
                foreach (FontFamily family in installed.Families)
                {
                    if (family.Name == familyName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Stream OpenResource(string path)
        {
            string resourceName = ResourcePrefix + path.Replace('/', '.').Replace('\\', '.');
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException("Embedded resource not found: " + resourceName);
            }

            return stream;
        }
    }
}
