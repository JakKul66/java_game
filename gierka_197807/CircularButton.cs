using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;

namespace gierka_197807
{
    public class CircularButton : Button
    {
        public int DotNumber { get; set; }
        public DotColor LogicColor { get; set; }
        public ContextItem ItemType { get; set; }

        // obrazki
        public Image CustomImage { get; set; }

        public CircularButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = new Font("Arial", 10, FontStyle.Bold);
            this.ForeColor = Color.Black;
            this.Cursor = Cursors.Hand;
            this.Size = new Size(90, 90);
            this.BackColor = Color.WhiteSmoke;
        }

        // robienie kolka
        protected override void OnPaint(PaintEventArgs pevent)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            this.Region = new Region(graphicsPath);

            // tlo
            base.OnPaint(pevent);

            // rysowanie obrazku
            if (CustomImage != null)
            {
                // grafika
                pevent.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pevent.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // marginesy zeby kolka nie nachodzily na siebie
                int margin = 15;
                Rectangle rect = new Rectangle(margin, margin, ClientSize.Width - 2 * margin, ClientSize.Height - 2 * margin);

                pevent.Graphics.DrawImage(CustomImage, rect);
            }
        }
    }
}