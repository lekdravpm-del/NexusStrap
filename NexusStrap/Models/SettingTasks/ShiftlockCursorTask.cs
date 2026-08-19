using NexusStrap.Enums;
using NexusStrap.Models.SettingTasks.Base;
using System.Windows.Media.Imaging;

namespace NexusStrap.Models.SettingTasks
{
    public class ShiftlockCursorTask : EnumBaseTask<CursorType>
    {
        private static readonly string[] ShiftlockFiles = { "MouseLockedCursor.png" };

        private static string ShiftlockPath => Path.Combine(Paths.Modifications, "content", "textures");

        public event EventHandler? PreviewChanged;

        public ShiftlockCursorTask() : base("ModPreset", "ShiftlockCursor") { }

        public override CursorType NewState
        {
            get => base.NewState;
            set
            {
                base.NewState = value;
                PreviewChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void Execute()
        {
            if (NewState == OriginalState)
                return;

            try
            {
                if (NewState == CursorType.Default)
                {
                    // Revert to the cursor theme's default shift-lock cursor
                    DeleteAppliedShiftlock();
                }
                else
                {
                    string resource = $"Cursor.{NewState}.MouseLockedCursor.png";

                    if (!Resource.Exists(resource))
                    {
                        App.Logger.WriteLine("ShiftlockCursorTask::Execute", $"Shift-lock resource not found: {resource}");
                        return;
                    }

                    Directory.CreateDirectory(ShiftlockPath);

                    using var resourceStream = Resource.GetStream(resource);
                    string dest = Path.Combine(ShiftlockPath, "MouseLockedCursor.png");
                    Filesystem.AssertReadOnly(dest);
                    using var fileStream = File.Create(dest);
                    resourceStream.CopyTo(fileStream);

                    App.Logger.WriteLine("ShiftlockCursorTask::Execute", $"Applied shift-lock theme {NewState}");
                }

                OriginalState = NewState;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ShiftlockCursorTask::Execute", ex);
            }
        }

        public void DeleteAppliedShiftlock()
        {
            string file = Path.Combine(ShiftlockPath, "MouseLockedCursor.png");
            if (File.Exists(file))
            {
                Filesystem.AssertReadOnly(file);
                File.Delete(file);
            }
        }

        public BitmapImage GetPreviewImage(CursorType theme)
        {
            if (theme == CursorType.Default)
                return null!;

            string resource = $"Cursor.{theme}.MouseLockedCursor.png";

            try
            {
                using var stream = Resource.GetStream(resource);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ShiftlockCursorTask::GetPreviewImage", ex);
                return null!;
            }
        }
    }
}