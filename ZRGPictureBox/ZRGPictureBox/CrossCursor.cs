using Summer.System.Log;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRGPictureBox
{
    public class CrossCursor
    {
        /// <summary>
        /// Cross cursor default size
        /// </summary>

        public static readonly Size DefaultSize = new Size(20, 20);

        #region "Variabili private"

        /// <summary>
        /// Controllo su cui disegnare il cursore
        /// </summary>

        private ZRGPictureBoxControl myPictureBox;
        /// <summary>
        /// Dimensione del cursore a croce
        /// </summary>

        private Size mySize = DefaultSize;
        /// <summary>
        /// Flag che indica se la croce va disegnata in modalita' "schermo pieno"
        /// </summary>

        private bool myFullPictureBoxCross = false;
        /// <summary>
        /// Colore con cui disegnare la croce
        /// </summary>

        private Color myColor = System.Drawing.Color.Black;
        /// <summary>
        /// Posizione su cui viene disegnata la croce [coordinate logiche]
        /// </summary>

        private System.Drawing.Point myCrossPosition = RECT.InvalidPoint();
        /// <summary>
        /// Rettangolo contenente i quattro punti corrispondenti all'ultima croce disegnata
        /// </summary>
        private System.Drawing.Point myLastCrossTopPoint;
        private System.Drawing.Point myLastCrossLeftPoint;
        private System.Drawing.Point myLastCrossRightPoint;

        private System.Drawing.Point myLastCrossBottomPoint;
        /// <summary>
        /// Box in cui vengono disegnate le coordinate, viene disegnata solo la parte di cursore che non cade sopra ad esso
        /// </summary>

        private CoordinatesBox myCoordinatesBox = null;
        #endregion

        #region "Proprieta'"

        /// <summary>
        /// PictureBoxControl associato a questa istanza delle classe
        /// </summary>
        public ZRGPictureBoxControl PictureBoxControl
        {
            get { return myPictureBox; }
            private set { myPictureBox = value; }
        }

        /// <summary>
        /// Dimensione del cursore a croce
        /// </summary>
        public Size Size
        {
            get { return mySize; }
            set { mySize = value; }
        }

        /// <summary>
        /// Colore del cursore a croce
        /// </summary>
        public Color Color
        {
            get { return myColor; }
            set { myColor = value; }
        }
        /// <summary>
        /// Box in cui vengono disegnate le coordinate, viene disegnata solo la parte di cursore che non cade sopra ad esso
        /// </summary>
        internal CoordinatesBox CoordinatesBox
        {
            get { return myCoordinatesBox; }
            set { myCoordinatesBox = value; }
        }

        #endregion

        #region "Costruttori"

        /// <summary>
        /// Costruttore dato un controllo su cui disegnare il cursore
        /// </summary>
        public CrossCursor(ZRGPictureBoxControl picPictureBox)
        {
            myPictureBox = picPictureBox;
        }

        #endregion

        #region "Funzioni pubbliche"
        /// <summary>
        /// Cancella la posizione su cui viene disegnata la croce. 
        /// </summary>
        public void ResetCrossPosition()
        {
            myCrossPosition = RECT.InvalidPoint();
        }

        /// <summary>
        /// Posizione su cui viene disegnata la croce [coordinate logiche]
        /// </summary>
        public System.Drawing.Point CrossPosition
        {
            get { return myCrossPosition; }
            set { myCrossPosition = value; }
        }

        /// <summary>
        /// Disegna la croce nella posizione data da CrossPosition
        /// </summary>
        internal void DrawCross(Graphics GR)
        {
            DrawCross(GR, CrossPosition);
        }

        /// <summary>
        /// Disegna la croce nella posizione specificata
        /// </summary>
        internal void DrawCross(Graphics GR, System.Drawing.Point LogicalCoord)
        {

            try
            {
                // Check se ho un controllo associato valido
                if (myPictureBox == null)
                {
                    return;
                }

                // Check se la coordinata passatami e' valida
                if (LogicalCoord == RECT.InvalidPoint())
                {
                    return;
                }

                // Posizione in cui disegnare la croce [coordinate fisiche della pictureBox]
                Point physicalCrossCoords = PictureBoxControl.GraphicInfo.ToPhysicalPoint(LogicalCoord);

                // Minimi e massimi valori permessi per i bracci della croce
                Point minCrossValue = Point.Empty;
                Point maxCrossValue = new Point(myPictureBox.Width, myPictureBox.Height);

                // Se ho un box delle coordinate, faccio in modo di non disegnarci sopra
                if (myCoordinatesBox != null)
                {
                    // Controllo che la croce (non a schermo pieno) non cada completamente dentro al box delle coordinate
                    // Se e' completamente dentro, non serve disegnare la croce
                    if (!myFullPictureBoxCross && myCoordinatesBox.DrawingRect.Contains(physicalCrossCoords))
                    {
                        return;
                    }

                    if (physicalCrossCoords.X > myCoordinatesBox.DrawingRect.X)
                    {
                        maxCrossValue.Y -= myCoordinatesBox.DrawingRect.Height;
                    }
                    if (physicalCrossCoords.Y > myCoordinatesBox.DrawingRect.Y)
                    {
                        maxCrossValue.X -= myCoordinatesBox.DrawingRect.Width;
                    }
                }

                // Due valori che utilizzo spesso
                int maxCrossValueX = maxCrossValue.X;
                //- 2
                int maxCrossValueY = maxCrossValue.Y;
                //- 2

                // Calcolo la posizione della nuova croce
                if (myFullPictureBoxCross)
                {
                    // Linea orizzontale
                    myLastCrossLeftPoint.X = minCrossValue.X;
                    myLastCrossRightPoint.X = maxCrossValue.X;
                    myLastCrossLeftPoint.Y = physicalCrossCoords.Y;
                    myLastCrossRightPoint.Y = physicalCrossCoords.Y;
                    // Linea verticale
                    myLastCrossTopPoint.Y = minCrossValue.Y;
                    myLastCrossBottomPoint.Y = maxCrossValue.Y;
                    myLastCrossTopPoint.X = physicalCrossCoords.X;
                    myLastCrossBottomPoint.X = physicalCrossCoords.X;
                }
                else
                {
                    // Linea orizzontale
                    myLastCrossLeftPoint.X = physicalCrossCoords.X - mySize.Width / 2;
                    myLastCrossRightPoint.X = physicalCrossCoords.X + mySize.Width / 2;
                    myLastCrossLeftPoint.Y = physicalCrossCoords.Y;
                    myLastCrossRightPoint.Y = physicalCrossCoords.Y;
                    // Linea verticale
                    myLastCrossTopPoint.Y = physicalCrossCoords.Y - mySize.Height / 2;
                    myLastCrossBottomPoint.Y = physicalCrossCoords.Y + mySize.Height / 2;
                    myLastCrossTopPoint.X = physicalCrossCoords.X;
                    myLastCrossBottomPoint.X = physicalCrossCoords.X;
                }

                // Controllo che la croce non debordi dalla PictureBox
                // Va fatto anche nel caso della croce a pieno schermo, perche' potrebbe debordare
                // nel caso in cui la pictureBox non occupa tutto lo spazio disponibile nell'applicazione.
                // In questo caso io andrei a disegnare sopra gli altri controlli dell'applicazione
                if (myLastCrossRightPoint.X > maxCrossValueX)
                    myLastCrossRightPoint.X = maxCrossValueX;
                if (myLastCrossRightPoint.Y > maxCrossValueY)
                    myLastCrossRightPoint.Y = maxCrossValueY;
                if (myLastCrossRightPoint.Y < minCrossValue.Y)
                    myLastCrossRightPoint.Y = minCrossValue.Y;
                if (myLastCrossBottomPoint.Y > maxCrossValueY)
                    myLastCrossBottomPoint.Y = maxCrossValueY;
                if (myLastCrossBottomPoint.X > maxCrossValueX)
                    myLastCrossBottomPoint.X = maxCrossValueX;
                if (myLastCrossBottomPoint.X < minCrossValue.X)
                    myLastCrossBottomPoint.X = minCrossValue.X;
                if (myLastCrossLeftPoint.X < minCrossValue.X)
                    myLastCrossLeftPoint.X = minCrossValue.X;
                if (myLastCrossLeftPoint.Y > maxCrossValueY)
                    myLastCrossLeftPoint.Y = maxCrossValueY;
                if (myLastCrossLeftPoint.Y < minCrossValue.Y)
                    myLastCrossLeftPoint.Y = minCrossValue.Y;
                if (myLastCrossTopPoint.Y < minCrossValue.Y)
                    myLastCrossTopPoint.Y = minCrossValue.Y;
                if (myLastCrossTopPoint.X > maxCrossValueX)
                    myLastCrossTopPoint.X = maxCrossValueX;
                if (myLastCrossTopPoint.X < minCrossValue.X)
                    myLastCrossTopPoint.X = minCrossValue.X;

                using (Pen crossPen = new Pen(myColor))
                {
                    GR.DrawLine(crossPen, myLastCrossLeftPoint, myLastCrossRightPoint);
                    GR.DrawLine(crossPen, myLastCrossTopPoint, myLastCrossBottomPoint);
                }

            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<CrossCursor>().Error(ex.Message);
                LogHelper.GetLogger<CrossCursor>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
                //MsgBox(ex.Message)
            }
        }

        #endregion
    }
}
