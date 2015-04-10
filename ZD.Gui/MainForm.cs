using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    /// <summary>
    /// The application's main form.
    /// </summary>
    public class MainForm : ZenTabbedForm
    {
        /// <summary>
        /// Main tab control: lookup.
        /// </summary>
        private LookupControl lc;
        /// <summary>
        /// Main tab control: settings ("Zydeo" tab).
        /// </summary>
        private SettingsControl stgs;
        /// <summary>
        /// Localized UI strings provider.
        /// </summary>
        private ITextProvider tprov;
        /// <summary>
        /// True if saved size and location didn't make sense and we reverted to defaults.
        /// </summary>
        private readonly bool ignoredSavedSizeAndLocation;
        /// <summary>
        /// True if updater should be launched when form is closed.
        /// </summary>
        private bool updateAfterClose = false;

        /// <summary>
        /// Ctor: initializes main form.
        /// </summary>
        public MainForm(ICedictEngineFactory dictFact, ITextProvider tprov)
            : base(tprov)
        {
            this.tprov = tprov;

            // Initialize hanzi renderer
            // -- Scale (DPI)
            // -- Available systems fonts
            HanziRenderer.Scale = Scale;
            if (HanziRenderer.IsWinKaiAvailable()) Magic.SetZhoContentFontFamily(IdeoFamily.WinKai);
            else Magic.SetZhoContentFontFamily(IdeoFamily.ArphicKai);

            // Initialize system font provider with our own
            if (SystemFontProvider.Instance as ZydeoSystemFontProvider == null)
                SystemFontProvider.Instance = new ZydeoSystemFontProvider();

            // Find out last window size and location from settings
            Size size = AppSettings.WindowLogicalSize;
            Point loc = AppSettings.WindowLoc;
            ignoredSavedSizeAndLocation = !verifySizeAndLoc(size, loc);
            // If location+size do not make sense, let system position window, and go with default size.
            if (ignoredSavedSizeAndLocation)
            {
                WinForm.StartPosition = FormStartPosition.WindowsDefaultLocation;
                LogicalSize = Magic.WinDefaultLogicalSize;
            }
            // Otherwise, position at last location
            else
            {
                WinForm.StartPosition = FormStartPosition.Manual;
                Location = loc;
                LogicalSize = size;
            }
            // Set (logical) minimum size
            LogicalMinimumSize = Magic.WinMinimumLogicalSize;

            Header = tprov.GetString("WinHeader");
            lc = new LookupControl(this, dictFact, tprov);
            stgs = new SettingsControl(this, tprov, dictFact);
            MainTab = new ZenTab(stgs, tprov.GetString("TabMain"));
            Tabs.Add(new ZenTab(lc, tprov.GetString("TabLookup")));
        }

        /// <summary>
        /// <para>Shows welcome screen in form informing user about an available update.</para>
        /// <para>Has no effect if user settings say no update notifications.</para>
        /// </summary>
        public void SetWelcomeUpdate(int vmaj, int vmin, DateTime rdate, string rnotes)
        {
            if (!AppSettings.NotifyOfUpdates) return;
            lc.SetWelcomeUpdate(vmaj, vmin, rdate, rnotes, updateNow);
        }

        /// <summary>
        /// Gets whether Zydeo updater should be launched after closing window.
        /// </summary>
        public bool UpdateAfterClose
        {
            get { return updateAfterClose; }
        }

        /// <summary>
        /// Handles user's action to close and update.
        /// </summary>
        private void updateNow()
        {
            updateAfterClose = true;
            WinForm.Close();
        }

        /// <summary>
        /// Verifies if size+location combo makes sense now (e.g., not on missing second monitor).
        /// </summary>
        private bool verifySizeAndLoc(Size size, Point loc)
        {
            // Minimum value means no previous real settings have been stored.
            if (loc.X == int.MinValue || loc.Y == int.MinValue) return false;
            // Below minimum size: no way.
            if (size.Width < Magic.WinMinimumLogicalSize.Width || size.Height < Magic.WinMinimumLogicalSize.Height)
                return false;
            // Enough of window must be on at least one screen.
            // I.e., Y is not negative, and Y is large, at least minimum height's top tenth's worth
            //   is visible on a screen where at least minimum width's one tenth's worth is visible.
            bool oneScreenOk = false;
            Rectangle wrect = new Rectangle(loc, size);
            int wTenth = (int)(((float)Magic.WinMinimumLogicalSize.Width * Scale * 0.1F));
            int hTenth = (int)(((float)Magic.WinMinimumLogicalSize.Height * Scale * 0.1F));
            int yAtTenth = wrect.Top + hTenth;
            foreach (Screen screen in Screen.AllScreens)
            {
                Rectangle isect = screen.WorkingArea;
                isect.Intersect(wrect);
                // At least one tenth of width is on this screen?
                if (isect.Width < wTenth) continue;
                // Is top of window greater than top of this screen?
                if (wrect.Top < screen.WorkingArea.Top) continue;
                // Is at least one tenth of window's top visible on this screen?
                if (yAtTenth > screen.WorkingArea.Bottom) continue;
                // OK, this screen will do
                oneScreenOk = true;
                break;
            }
            // Not OK on any screen - no good
            return oneScreenOk;
        }

        /// <summary>
        /// Handles form loaded event
        /// </summary>
        /// <remarks>
        /// Saves window size and location that we ended up with if we didn't used size and location from settings.
        /// </remarks>
        protected override void OnFormLoaded()
        {
            base.OnFormLoaded();
            if (ignoredSavedSizeAndLocation)
                AppSettings.SetWindowSizeAndLocation(Location, LogicalSize);
        }

        /// <summary>
        /// Handes move/resize finished event: save current window size and location.
        /// </summary>
        protected override void DoMoveResizeFinished()
        {
            AppSettings.SetWindowSizeAndLocation(Location, LogicalSize);
        }

        /// <summary>
        /// Force-closes and disposes the window.
        /// </summary>
        public void ForceClose()
        {
            if (WinForm.InvokeRequired)
            {
                InvokeOnForm((MethodInvoker)delegate
                {
                    WinForm.Close();
                });
                return;
            }
            WinForm.Close();
        }
    }
}
