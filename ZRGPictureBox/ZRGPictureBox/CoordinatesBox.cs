using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRGPictureBox
{
    public partial class CoordinatesBox : UserControl
    {
        private ZRGPictureBoxControl myPictureBoxControl;
        private Rectangle myDrawingRect = Rectangle.Empty;

        private Point myLastCoordToDraw = new Point(int.MaxValue, int.MaxValue);

        public ZRGPictureBoxControl PictureBoxControl
        {
            get { return myPictureBoxControl; }
            private set { myPictureBoxControl = value; }
        }
        public MeasureSystem.enUniMis UnitOfMeasure
        {
            get { return PictureBoxControl.UnitOfMeasure; }
        }
        public Rectangle DrawingRect
        {
            get { return myDrawingRect; }
        }
        private float UnitOfMeasureFactor
        {
            get { return MeasureSystem.CustomUnitToMicron(1, UnitOfMeasure); }
        }
        private string UnitOfMeasureString
        {
            get { return MeasureSystem.UniMisDescription(UnitOfMeasure); }
        }

        public CoordinatesBox()
        {
            InitializeComponent();
        }

        public CoordinatesBox(ZRGPictureBoxControl pictureBox)
        {
            myPictureBoxControl = pictureBox;
        }

        Font static_DrawCoordinateInfo_textFont;
        int static_DrawCoordinateInfo_borderSize;

        // Se il box cambia dimensioni, invalido la picturebox, cosi' sembra sempre che sia pulito
        // Serve nel caso in cui:
        //  - si passa da "X=100,Y=100" a "X=99,Y=99", per cui rimarrebbe una parte del box precedente "scoperta"
        //  - si cambia unita' di misura a runtime, quindi il box viene ridimensionato
        SizeF static_DrawCoordinateInfo_oldTextBox;
        public void DrawCoordinateInfo(Graphics GR, Point CoordToDraw, bool PixelCoordMode = false)
        {
            try
            {
                if (myPictureBoxControl == null)
                {
                    return;
                }
                if (GR == null)
                {
                    return;
                }
                if (CoordToDraw.X == int.MaxValue || CoordToDraw.Y == int.MaxValue)
                {
                    return;
                }
                myLastCoordToDraw = CoordToDraw;
                static_DrawCoordinateInfo_textFont = new Font("Arial narrow", 8);
                static_DrawCoordinateInfo_borderSize = (int)Math.Ceiling(GR.MeasureString("_", static_DrawCoordinateInfo_textFont).Width / 2);
                float _umsf = UnitOfMeasureFactor;
                if (PixelCoordMode)
                {
                    _umsf = 1;
                }
                float xValue = CoordToDraw.X / _umsf;
                float yValue = CoordToDraw.Y / _umsf;
                string textToDraw = null;
                if (PixelCoordMode)
                {
                    textToDraw = "X=" + xValue.ToString("0000.00") + ", Y=" + yValue.ToString("0000.00");
                }
                else
                {
                    if (UnitOfMeasure != MeasureSystem.enUniMis.micron)
                    {
                        textToDraw = "X=" + xValue.ToString("0000.00") + ", Y=" + yValue.ToString("0000.00") + UnitOfMeasureString;
                    }
                    else
                    {
                        textToDraw = "X=" + xValue.ToString("0000") + ", Y=" + yValue.ToString("0000") + UnitOfMeasureString;
                    }
                }
                SizeF textBox = GR.MeasureString(textToDraw, static_DrawCoordinateInfo_textFont);
                static_DrawCoordinateInfo_oldTextBox = textBox;
                if (static_DrawCoordinateInfo_oldTextBox != textBox)
                {
                    // Aggiorno le dimensioni precedenti del box
                    static_DrawCoordinateInfo_oldTextBox = textBox;
                }

                // Aggiorno le coordinate del rettangolo di sfondo
                // NOTA: Uso il ClientRectangle.Width al posto di Width perche' cosi' tengo conto delle eventuali scrollbar.
                myDrawingRect.X = (int)(myPictureBoxControl.ClientRectangle.Width - textBox.Width - static_DrawCoordinateInfo_borderSize);
                myDrawingRect.Y = (int)(myPictureBoxControl.ClientRectangle.Height - textBox.Height - static_DrawCoordinateInfo_borderSize);
                myDrawingRect.Width = (int)(textBox.Width + static_DrawCoordinateInfo_borderSize);
                myDrawingRect.Height = (int)(textBox.Height + static_DrawCoordinateInfo_borderSize);

                // Se le scrollbar sono visibili, devo fare in modo che sia visibile anche il bordo inferiore/destro del rettangolo
                if (myPictureBoxControl.HScrollable)
                {
                    myDrawingRect.Height -= 1;
                }
                if (myPictureBoxControl.VScrollable)
                {
                    myDrawingRect.Width -= 1;
                }

                // Disegno il rettangolo di sfondo
                GR.FillRectangle(Brushes.White, myDrawingRect);
                GR.DrawRectangle(Pens.Black, myDrawingRect);

                // Disegno la stringa di testo, l'aggiunta di borderSize/2 serve a centrare il testo nel rettangolo di sfondo 
                GR.DrawString(textToDraw, static_DrawCoordinateInfo_textFont, Brushes.Black, myDrawingRect.X + static_DrawCoordinateInfo_borderSize / 2, myDrawingRect.Y + static_DrawCoordinateInfo_borderSize / 2);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
