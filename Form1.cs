using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Transitions;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        Point OriginalLocation;
        Point OriginalControlLocation;
        Control ActiveControl;


        private Control _closestControl;
        private const int SnapDist = 10;
        public Form1()
        {
            InitializeComponent();

            foreach (var btn in Controls.OfType<Button>())
            {
                btn.MouseDown += Btn_MouseDown;
                btn.MouseUp += Btn_MouseUp;
                btn.MouseMove += Button_MouseMove;
            }
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (ActiveControl == null || ActiveControl != sender) return;
            if (e.Button != MouseButtons.Left) return;

            var newLocation = ActiveControl.Location + (Size)e.Location - (Size)OriginalLocation;
            var direction = new Size(e.Location.X - this.OriginalLocation.X, e.Location.Y - this.OriginalLocation.Y);
            Control closestControl = null;
            double closestDist = double.MaxValue;

            
            foreach (Control control in Controls)
            {
                // Reset ForeColor of all controls to default
                control.ForeColor = DefaultForeColor;
                if (control == ActiveControl)
                    continue;

                // Find the closest control
                double dist = Math.Sqrt(Math.Pow(control.Location.X - newLocation.X, 2) + Math.Pow(control.Location.Y - newLocation.Y, 2));
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestControl = control;
                    Debug.WriteLine($"Distance from closest control : {closestDist:N2}");
                }
            }

            // If a closest control is found, change its ForeColor to Red
            if (closestControl != null)
            {
                closestControl.ForeColor = Color.Red;
            }

            // If the active control intersects with the closest control, don't allow movement
            if (closestControl != null && new Rectangle(newLocation, ActiveControl.Size).IntersectsWith(closestControl.Bounds))
            {
                Cursor.Current = Cursors.No;
                //return;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            var distLeft = Math.Abs(closestControl.Right - newLocation.X);
            var distRight = Math.Abs(closestControl.Left - newLocation.X - ActiveControl.Width);
            var distTop = Math.Abs(closestControl.Bottom - newLocation.Y);
            var distBottom = Math.Abs(closestControl.Top - newLocation.Y - ActiveControl.Height);

            // Snap the active control to the closest control if within snap distance
            if (direction.Width > 0 && distRight < SnapDist) // Moving right
            {
                newLocation.X = closestControl.Left - ActiveControl.Width - SnapDist;
            }
            else if (direction.Width < 0 && distLeft < SnapDist) // Moving left
            {
                newLocation.X = closestControl.Right + SnapDist;
            }
            if (direction.Height > 0 && distBottom < SnapDist) // Moving down
            {
                newLocation.Y = closestControl.Top - ActiveControl.Height - SnapDist;
            }
            else if (direction.Height < 0 && distTop < SnapDist) // Moving up
            {
                newLocation.Y = closestControl.Bottom + SnapDist;
            }

            // Align the active control to the closest control if close on X or Y axis
            if (Math.Abs(newLocation.X - closestControl.Location.X) < SnapDist)
            {
                newLocation.X = closestControl.Location.X;
            }
            if (Math.Abs(newLocation.Y - closestControl.Location.Y) < SnapDist)
            {
                newLocation.Y = closestControl.Location.Y;
            }

            ActiveControl.Location = newLocation;
        }


        private void Btn_MouseUp(object? sender, MouseEventArgs e)
        {
            // Si le bouton est relâché au-dessus d'un autre bouton, retournez à la position originale
            foreach (Control control in Controls)
            {
                if (control == ActiveControl)
                    continue;

                if (new Rectangle(ActiveControl.Location, ActiveControl.Size).IntersectsWith(control.Bounds))
                {
                    Transition t = new(new TransitionType_EaseInEaseOut(200));
                    t.add(ActiveControl, "Left", OriginalControlLocation.X);
                    t.add(ActiveControl, "Top", OriginalControlLocation.Y);
                    t.run();
                }
            }
            ActiveControl = null;
            Cursor.Current = Cursors.Default;
        }

        private void Btn_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ActiveControl = sender as Button;
            OriginalControlLocation = ActiveControl.Location;
            OriginalLocation = e.Location;
            Cursor.Current = Cursors.Hand;

        }
    }
}