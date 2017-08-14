using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRGPictureBox
{
    public class SelectionBoxElement
    {
        #region "Variabili private"
        public System.Drawing.Point TopLeftCorner = System.Drawing.Point.Empty;
        public System.Drawing.Point BottomRightCorner = RECT.InvalidPoint();
        public bool KeepAspectRatio = false;
        public ZRGPictureBoxControl LinkedPictureBox;
        private static Pen myBoxPenAreaSelection = new Pen(Color.FromArgb(200, Color.Black));
        private static Pen myBoxPenSingleClick = new Pen(Color.FromArgb(200, Color.Red));
        #endregion
        private static SolidBrush myBoxBrush = new SolidBrush(Color.FromArgb(40, Color.CadetBlue));

        #region "Proprieta'"
        public bool IsInvalid
        {
            get { return BottomRightCorner == RECT.InvalidPoint() || TopLeftCorner == RECT.InvalidPoint(); }
        }
        /// <summary>
        /// Ritorna la dimensione dell'area che va selezionata nel caso di selezione tramite singolo click.
        /// </summary>
        private int PointSelectAreaSize
        {
            // Calcolo l'area da tenere attorno al punto, tengo 15 pixel in tutto
            get { return (int)LinkedPictureBox.GraphicInfo.ToLogicalDimension(15f); }
        }
        private RECT SingleClickRectangle
        {
            get
            {
                int halfAreaSize = PointSelectAreaSize / 2;
                RECT r = new RECT(TopLeftCorner.X - halfAreaSize, TopLeftCorner.Y - halfAreaSize, TopLeftCorner.X + halfAreaSize, TopLeftCorner.Y + halfAreaSize);
                r.NormalizeRect();
                return r;
            }
        }
        public bool IsCreatedFromSinglePoint
        {
            get
            {
                // Check se il rettangolo ha entrambe le coordinate valide
                if (IsInvalid)
                {
                    return false;
                }
                // Check se le coordinate sono uguali
                if ((TopLeftCorner == BottomRightCorner))
                {
                    return true;
                }
                // Se il "rettangolo da singolo click" contiene il secondo punto del box,
                // allora il box di selezione e' stato creato tramite un singolo click
                return SingleClickRectangle.Contains(ref BottomRightCorner);
            }
        }
        #endregion

        #region "Operatori"
        public static implicit operator RECT(SelectionBoxElement box)
        {
            if (box.IsInvalid)
            {
                return new RECT();
            }
            if (box.IsCreatedFromSinglePoint)
            {
                return box.SingleClickRectangle;
            }
            else
            {
                return box.RectFromPoints(box.TopLeftCorner, box.BottomRightCorner);
            }
        }
        #endregion

        #region "Costruttori"
        public SelectionBoxElement(ZRGPictureBoxControl picBox)
        {
            LinkedPictureBox = picBox;
        }

        #endregion

        #region "Funzioni private"
        private RECT RectFromPoints(System.Drawing.Point FirstCorner, System.Drawing.Point SecondCorner)
        {
            try
            {
                if (FirstCorner == RECT.InvalidPoint() || SecondCorner == RECT.InvalidPoint())
                {
                    return new RECT();
                }

                if (KeepAspectRatio)
                {
                    int Sign = 0;
                    if ((Math.Abs((SecondCorner.X - FirstCorner.X) / LinkedPictureBox.Width)) > Math.Abs(((SecondCorner.Y - FirstCorner.Y) / LinkedPictureBox.Height)))
                    {
                        if (SecondCorner.Y > FirstCorner.Y)
                            Sign = 1;
                        else
                            Sign = -1;
                        SecondCorner.Y = FirstCorner.Y + Math.Abs((SecondCorner.X - FirstCorner.X) * (LinkedPictureBox.Height / LinkedPictureBox.Width)) * Sign;
                    }
                    else
                    {
                        if (SecondCorner.X > FirstCorner.X)
                            Sign = 1;
                        else
                            Sign = -1;
                        SecondCorner.X = FirstCorner.X + Math.Abs((SecondCorner.Y - FirstCorner.Y) * (LinkedPictureBox.Width / LinkedPictureBox.Height)) * Sign;
                    }
                }

                RECT r = new RECT(FirstCorner.X, FirstCorner.Y, SecondCorner.X, SecondCorner.Y);
                r.NormalizeRect();

                return r;
            }
            catch (Exception e)
            {
                //Interaction.MsgBox(e.Message);
                return new RECT();
            }
        }

        #endregion

        #region "Funzioni pubbliche"
        public void Reset()
        {
            TopLeftCorner = RECT.InvalidPoint();
            BottomRightCorner = RECT.InvalidPoint();
        }
        public void Draw(Graphics GR, bool usePhysicalCoords = true)
        {
            // Check se questo box e' valido
            if (this.IsInvalid)
            {
                return;
            }

            // Trovo il rettangolo da invalidare
            RECT r = this;

            // Se serve, converto in cordinate fisiche
            if (usePhysicalCoords)
            {
                r = LinkedPictureBox.GraphicInfo.ToPhysicalRect(r);
            }

            // Check se il rettangolo ottenuto e' valido
            if (r.IsZeroSized)
            {
                return;
            }

            // Disegno il rettangolo
            if (this.IsCreatedFromSinglePoint)
            {
                GR.DrawRectangle(myBoxPenSingleClick, r);
            }
            else
            {
                GR.FillRectangle(myBoxBrush, r);
                GR.DrawRectangle(myBoxPenAreaSelection, r);
            }
        }
        public void Invalidate()
        {
            RECT r = this;
            // NOTA: Alla Invalidate() vanno passate coordinate fisiche, non logiche
            r = LinkedPictureBox.GraphicInfo.ToPhysicalRect(r);
            r.Inflate(1, 1);
            // Effettuo il ridisegno della PictureBox associata
            LinkedPictureBox.Invalidate(r);
        }
        #endregion
    }
}
