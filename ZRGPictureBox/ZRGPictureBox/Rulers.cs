using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ZRGPictureBox
{
    public partial class Rulers : UserControl
    {
        #region "Costanti"
        public const int RulerSize = 20;
        private const double FreeSpaceFactor = 1.75;
        #endregion
        private const int RulerColorAlpha = 130;

        private class DrawNumberBitmap
        {

            #region "Costanti"

            /// <summary>
            /// Larghezza di una singola cifra [pixel]
            /// </summary>

            private const int DigitWidth = 6;
            #endregion

            #region "Variabili shared"

            /// <summary>
            /// Tabella contenente gli array di coordinate relativi alle varie cifre (e segni)
            /// </summary>

            static Dictionary<char, Point[]> signsTable = new Dictionary<char, Point[]>();
            #endregion

            #region "Funzioni private"

            /// <summary>
            /// Inizializza gli array di pixel corrispondenti alle varie cifre
            /// </summary>

            private static void Initialize()
            {
                // Array delle coordinate associate alle cifre (o ai segni) da tracciare
                // Ogni coppia di coordinate indica uno degli estremi dei segmenti 
                // che devo tracciare per ottenere la cifra (o il segno) voluto

                // Cifra "1"
                Point[] one = {
                    new Point(0, 2),
                    new Point(2, 0),
                    new Point(2, 7)
                };

                // Cifra "2"
                Point[] two = {
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(3, 0),
                    new Point(4, 1),
                    new Point(4, 3),
                    new Point(0, 7),
                    new Point(4, 7)
                };

                // Cifra "3"
                Point[] three = {
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(3, 0),
                    new Point(4, 1),
                    new Point(4, 2),
                    new Point(3, 3),
                    new Point(2, 3),
                    new Point(3, 3),
                    new Point(4, 4),
                    new Point(4, 6),
                    new Point(3, 7),
                    new Point(1, 7),
                    new Point(0, 6)
                };

                // Cifra "4"
                Point[] four = {
                    new Point(4, 5),
                    new Point(0, 5),
                    new Point(0, 4),
                    new Point(2, 1),
                    new Point(3, 0),
                    new Point(3, 7)
                };

                // Cifra "5"
                Point[] five = {
                    new Point(4, 0),
                    new Point(1, 0),
                    new Point(1, 1),
                    new Point(0, 2),
                    new Point(0, 3),
                    new Point(3, 3),
                    new Point(4, 4),
                    new Point(4, 6),
                    new Point(3, 7),
                    new Point(1, 7),
                    new Point(0, 6)
                };

                // Cifra "6"
                Point[] six = {
                    new Point(3, 0),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(0, 6),
                    new Point(1, 7),
                    new Point(3, 7),
                    new Point(4, 6),
                    new Point(4, 4),
                    new Point(3, 3),
                    new Point(0, 3)
                };

                // Cifra "7"
                Point[] seven = {
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(1, 7)
                };

                // Cifra "8"
                Point[] eight = {
                    new Point(3, 0),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(0, 2),
                    new Point(1, 3),
                    new Point(3, 3),
                    new Point(4, 4),
                    new Point(4, 6),
                    new Point(3, 7),
                    new Point(1, 7),
                    new Point(0, 6),
                    new Point(0, 4),
                    new Point(1, 3),
                    new Point(3, 3),
                    new Point(4, 2),
                    new Point(4, 1),
                    new Point(3, 0)
                };

                // Cifra "9"
                Point[] nine = {
                    new Point(0, 6),
                    new Point(1, 7),
                    new Point(3, 7),
                    new Point(4, 6),
                    new Point(4, 1),
                    new Point(3, 0),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(0, 3),
                    new Point(1, 4),
                    new Point(4, 4)
                };

                // Cifra "0"
                Point[] zero = {
                    new Point(1, 0),
                    new Point(3, 0),
                    new Point(4, 1),
                    new Point(4, 6),
                    new Point(3, 7),
                    new Point(1, 7),
                    new Point(0, 6),
                    new Point(0, 1),
                    new Point(1, 0)
                };

                // Segno "-"
                Point[] minus = {
                    new Point(1, 3),
                    new Point(4, 3)
                };

                // Segno "." e segno ","
                Point[] dot = {
                    new Point(2, 6),
                    new Point(3, 6),
                    new Point(2, 7),
                    new Point(3, 7)
                };

                // Aggiungo i vari array alla tabella
                signsTable.Add('1', one);
                signsTable.Add('2', two);
                signsTable.Add('3', three);
                signsTable.Add('4', four);
                signsTable.Add('5', five);
                signsTable.Add('6', six);
                signsTable.Add('7', seven);
                signsTable.Add('8', eight);
                signsTable.Add('9', nine);
                signsTable.Add('0', zero);
                signsTable.Add('-', minus);
                signsTable.Add('.', dot);
                signsTable.Add(',', dot);
            }

            /// <summary>
            /// Ritorna una serie di coordinate dei segmenti da tracciare per ottenere una rappresentazione grafica del numero passatogli.
            /// </summary>
            private void CreateSegmentsList(double Value, ref List<System.Drawing.Point> pointList, bool Horizontal, bool HideSign = false)
            {
                try
                {
                    // Check se devo ancora inizializzare gli array di pixel delle singole cifre
                    if (signsTable.Count == 0)
                    {
                        Initialize();
                    }

                    // Cancello la lista di punti
                    pointList.Clear();

                    // Converto il valore in stringa 
                    string strValue = ValueString(Value);

                    // Offset necessario per allineare la scritta al centro 
                    int alignmentOffset = -MaskWidth(Value) / 2;

                    // Array che conterra' i dati del carattere attuale
                    Point[] actualSign = null;
                    char actualChar = '\0';

                    // Scandisco tutti i carateri costituenti il numero da stampare
                    for (int actualIndex = 0; actualIndex <= strValue.Length - 1; actualIndex++)
                    {
                        actualChar = strValue.ToCharArray()[actualIndex];

                        // Se richiesto, salto il segno meno
                        if (HideSign && actualChar == '-')
                        {
                            continue;
                        }

                        // Recupero la tabella di coordinate associata al carattere attuale
                        actualSign = signsTable[actualChar];

                        System.Drawing.Point newPoint = default(System.Drawing.Point);
                        int xCoord = 0;
                        int yCoord = 0;
                        pointList.Capacity = pointList.Count + actualSign.Length + 1;
                        for (int i = 0; i <= actualSign.Length - 1; i++)
                        {
                            // Calcolo le coordinate del nuovo punto basandomi sul template
                            xCoord = (DigitWidth * actualIndex) + actualSign[i].X + alignmentOffset;
                            yCoord = actualSign[i].Y;
                            // Se la scritta e' verticale, scambio le coordinate
                            if (Horizontal)
                            {
                                newPoint = new System.Drawing.Point(xCoord, yCoord);
                            }
                            else
                            {
                                newPoint = new System.Drawing.Point(yCoord, -xCoord);
                            }
                            // Aggiungo il punto alla lista
                            pointList.Add(newPoint);
                        }
                        // Il punto con X e Y a maxvalues verra' ignorato 
                        pointList.Add(new System.Drawing.Point(int.MaxValue, int.MaxValue));
                    }

                }
                catch (Exception ex)
                {
                    //Interaction.MsgBox(ex.Message + Constants.vbCr + ex.StackTrace);
                }
            }

            #endregion

            #region "Proprieta'"

            /// <summary>
            /// Larghezza che avra' la maschera finale del valore passato [pixel]
            /// </summary>
            public int MaskWidth (double aValue)
            {
                // Larghezza di una cifra [pixel] * "numero di cifre nella rappresentazione come stringa"
                return DigitWidth * ValueString(aValue).Length;
            }

            /// <summary>
            /// Ritorna la stringa da stampare per il valore passatogli
            /// </summary>
            private string ValueString (double aValue)
            {
                // Converto in stringa in modo che mantenga almeno uno zero prima dei decimali
                // NOTA: Le 3 cifre dopo la virgola servono solo quando uso le inches e vado in zoom molto alti.
                //       In realta' per il livello di zoom massimo implementato nella PictureBox avrei 4 cifre
                //       dopo la virgola, ma la quarta cifra non e' molto precisa, quindi non la stampo
                return aValue.ToString("0.###");
            }

            private double aValue;

            List<System.Drawing.Point> static_DrawScaledNumber_pixelList;
            List<Point> static_DrawScaledNumber_logicCoordList;

            #endregion


            public void DrawScaledNumber(Graphics GR, double value, float xCoord, float yCoord, float ScaleFactor, bool Horizontal)
            {
                try
                {
                    // Calcolo le coordinate dei segmenti da tracciare
                    static_DrawScaledNumber_pixelList = new List<System.Drawing.Point>();
                    CreateSegmentsList(value, ref static_DrawScaledNumber_pixelList, Horizontal);

                    // Lista temporanea dei segmenti in coordinate logiche
                    static_DrawScaledNumber_logicCoordList = new List<Point>();
                    static_DrawScaledNumber_logicCoordList.Clear();

                    Point tmpLogicPoint = default(Point);
                    for (int iIter = 0; iIter <= static_DrawScaledNumber_pixelList.Count - 1; iIter++)
                    {
                        // Il tag a zero indica che sto processando i segmenti relativi ad una cifra
                        // Il tag messo a 1 segnala che e' finita una cifra (o un segno)
                        if (static_DrawScaledNumber_pixelList[iIter].X != int.MaxValue)
                        {
                            // Tutti i segmenti costituenti una cifra vanno convertiti in coordinate logiche
                            tmpLogicPoint.X = (int)(static_DrawScaledNumber_pixelList[iIter].X / ScaleFactor + xCoord);
                            tmpLogicPoint.Y = (int)(static_DrawScaledNumber_pixelList[iIter].Y / ScaleFactor + yCoord);
                            // Poi li salvo nella lista
                            static_DrawScaledNumber_logicCoordList.Add(tmpLogicPoint);
                        }
                        else
                        {
                            // Quando e' finita una cifra (o un segno), disegno l'array di segmenti corrispondente
                            GR.DrawLines(RulerPen, static_DrawScaledNumber_logicCoordList.ToArray());
                            // Poi resetto la lista per la prossima cifra
                            static_DrawScaledNumber_logicCoordList.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Interaction.MsgBox(ex.Message + Constants.vbCr + ex.StackTrace);
                }
            }

        }

        private ZRGPictureBoxControl withEventsField_myPictureBoxControl;
        private ZRGPictureBoxControl myPictureBoxControl
        {
            get { return withEventsField_myPictureBoxControl; }
            set
            {
                if (withEventsField_myPictureBoxControl != null)
                {
                    withEventsField_myPictureBoxControl.OnMeasureUnitChanged -= PictureBox_MeasureUnitChanged;
                }
                withEventsField_myPictureBoxControl = value;
                if (withEventsField_myPictureBoxControl != null)
                {
                    withEventsField_myPictureBoxControl.OnMeasureUnitChanged += PictureBox_MeasureUnitChanged;
                }
            }

        }

        private static Pen RulerPen = new Pen(Color.Navy);
        /// <summary>
        /// Penna che uso per disegnare le righe di drag and drop dei righelli
        /// </summary>

        private static Pen myDragPen = null;
        /// <summary>
        /// Bitmap che mi serve per l'origine
        /// </summary>

        private static Image myOriginBmp = null;
        /// <summary>
        /// Bitmap che mi serve per l'origine
        /// </summary>

        private static Bitmap myOriginBmpSnapped = null;
        /// <summary>
        /// Variabile utilizzata per la creazione delle maschere di pixel.
        /// NOTA: Ogni cifra che verra' disegnata nei righelli richiede la creazione di una maschera
        /// </summary>

        private static DrawNumberBitmap digitMaskCreator = new DrawNumberBitmap();

        /// <summary>
        /// Bitmap contenente il righello orizzontale
        /// </summary>

        private Bitmap myHRulerBmp = null;
        /// <summary>
        /// Bitmap contenente il righello verticale
        /// </summary>

        private Bitmap myVRulerBmp = null;

        /// <summary>
        /// Colore della penna che uso per disegnare le righe di drag and drop dei righelli
        /// </summary>

        Color myDragLineColor = Color.Black;

        /// <summary>
		/// Ampiezza dei righelli [pixel]
		/// </summary>
		private int mySize = RulerSize;

        /// <summary>
        /// Ultimi dati grafici (origine, dimensioni, fattore di scala, ecc.) con cui ho ridisegnato la bitmap dei righelli.
        /// </summary>

        private ConversionInfo myLastGraphicInfo = null;

        /// <summary>
        /// Flag usato per indicare quando c'e' bisogno di un ridisegno del righello orizzontale
        /// </summary>

        private bool NeedsHorizontalRedraw = true;
        /// <summary>
        /// Flag usato per indicare quando c'e' bisogno di un ridisegno del righello verticale
        /// </summary>

        private bool NeedsVerticalRedraw = true;

        /// <summary>
        /// PictureBox associata a questa istanza delle classe
        /// </summary>
        public ZRGPictureBoxControl PictureBoxControl
        {
            get { return myPictureBoxControl; }
            private set { myPictureBoxControl = value; }
        }
        /// <summary>
        /// Unita' di misura usata nel righello
        /// </summary>
        public MeasureSystem.enUniMis UnitOfMeasure
        {
            get { return PictureBoxControl.UnitOfMeasure; }
        }

        /// <summary>
        /// Larghezza del righello orizzontale [pixel]
        /// </summary>
        public int Width
        {
            get { return PictureBoxControl.Width; }
        }

        /// <summary>
        /// Altezza del righello verticale [pixel]
        /// </summary>
        public int Height
        {
            get { return PictureBoxControl.Height; }
        }

        /// <summary>
        /// Ampiezza dei righelli [pixel]
        /// </summary>
        public int Size
        {
            get { return mySize; }
            set
            {
                if (mySize == value)
                {
                    return;
                }
                mySize = value;
                // Ho cambiato una dimensione di entrambi i righelli, quindi devo ricrearne le bitmap da zero
                myHRulerBmp = null;
                myVRulerBmp = null;
            }
        }

        /// <summary>
        /// Larghezza del righello orizzontale [dimensioni logiche]
        /// </summary>
        public int LogicalWidth
        {
            get
            {
                if (ScaleFactor != 0)
                {
                    return (int)(Width / ScaleFactor);
                }
                return 0;
            }
        }

        /// <summary>
        /// Altezza del righello verticale [dimensioni logiche]
        /// </summary>
        public int LogicalHeight
        {
            get
            {
                if (ScaleFactor != 0)
                {
                    return (int)(Height / ScaleFactor);
                }
                return 0;
            }
        }

        /// <summary>
        /// Ampiezza dei righelli [dimensioni logiche]
        /// </summary>
        public int LogicalSize
        {
            get
            {
                if (ScaleFactor != 0)
                {
                    return (int)(Size / ScaleFactor);
                }
                return 0;
            }
        }

        /// <summary>
        /// Ritorna il fattore di scala con cui va disegnato questo oggetto
        /// </summary>
        public float ScaleFactor
        {
            get { return PictureBoxControl.ScaleFactor; }
        }

        /// <summary>
        /// Punto disegnato in alto a sinistra dello schermo [coordinate logiche]
        /// </summary>
        public Point LogicalOrigin
        {
            get { return PictureBoxControl.LogicalOrigin; }
        }

        /// <summary>
        /// Colore del righello
        /// </summary>
        public Color RulerColor
        {
            get { return Color.FromArgb(RulerColorAlpha, Color.LightYellow); }
        }
        /// <summary>
        /// Ritorna la larghezza che la bitmap deve avere per disegnare correttamente il righello orizzontale sulla PictureBox
        /// </summary>
        private int NeededBitmapWidth
        {
            get
            {
                // Devo avere una bitmap di dimensione pari a quello che devo disegnare
                int newWidth = PictureBoxControl.Width;
                // Per evitare di allocare e deallocare continuamente delle bitmap quando l'utente ridimensiona la finestra
                // trascinando con il mouse, le bitmap vengono allocate arrotondando ai 100 pixel superiori
                newWidth = (int)Math.Ceiling(Convert.ToDouble(newWidth) / 100.0) * 100;
                return newWidth;
            }
        }

        /// <summary>
        /// Ritorna l'altezza che la bitmap deve avere per disegnare correttamente il righello verticale sulla PictureBox
        /// </summary>
        private int NeededBitmapHeight
        {
            get
            {
                // Controllo se ho una pictureBox collegata a questi righelli
                // Devo avere una bitmap di dimensione pari a quello che devo disegnare
                int newHeight = PictureBoxControl.Height;
                // Per evitare di allocare e deallocare continuamente delle bitmap quando l'utente ridimensiona la finestra
                // trascinando con il mouse, le bitmap vengono allocate arrotondando ai 100 pixel superiori
                newHeight = (int)Math.Ceiling(Convert.ToDouble(newHeight) / 100.0) * 100;
                return newHeight;
            }
        }

        /// <summary>
        /// Ritorna la dimensione comune che la bitmap deve avere per disegnare correttamente i righelli.
        /// </summary>
        private int NeededBitmapRulerSize
        {
            // Devo avere una bitmap di dimensione pari a quello che devo disegnare
            get { return Size; }
        }

        public Rulers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Costruttore dato un controllo su cui disegnare il righello
        /// </summary>
        public Rulers(ZRGPictureBoxControl pictureBox)
        {
            myPictureBoxControl = pictureBox;

            // Creo la penna che uso per disegnare le righe di drag and drop dei righelli
            if (myDragPen == null)
            {
                CreateDragPen();
            }

            // Carico la bitmap che mi serve per l'origine
            myOriginBmp = global::ZRGPictureBox.Properties.Resources.Rulers_RulerOrigin;
            myOriginBmpSnapped = global::ZRGPictureBox.Properties.Resources.Rulers_RulerOriginSnap;
            //myOriginBmp = LoadImageRes("Rulers.RulerOrigin.png");
            //myOriginBmpSnapped = (Bitmap)LoadImageRes("Rulers.RulerOriginSnap.png");

            // Aggiorno la dimensione dei righelli in modo che siano coerenti con la dimensione della bitmap
            Size = Math.Max(myOriginBmp.Width, myOriginBmp.Height);
        }

        string[] static_LoadImageRes_resourcesNames;
        private Image LoadImageRes(string imageName)
        {
            try
            {
                System.Reflection.Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                string assemblyName = thisAssembly.GetName().Name;
                imageName = assemblyName + "." + imageName;
                static_LoadImageRes_resourcesNames = thisAssembly.GetManifestResourceNames();
                foreach (string name in static_LoadImageRes_resourcesNames)
                {
                    //Case insensitive
                    if (name.ToLower() == imageName.ToLower())
                    {
                        System.IO.Stream file = thisAssembly.GetManifestResourceStream(name);
                        return Image.FromStream(file);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Ritorna il valore del passo necessario pwer visualizzare correttamente le quote su di un righello
        /// </summary>
        private int CalculateBaseStep(bool horizontalRuler)
        {

            // NOTA: Si suppone che il numero piu' largo cada ad uno degli estremi
            //       dell'intervallo di valori visualizzato. In realta' questo non e' vero,
            //       perche' se ho numeri con la virgola (con zoom alti e inches, ad esempio)
            //       posso avere come estremi 2 e 3, ed in mezzo trovarmi con 2.555 e 2.666.
            //       Comunque scelgo di ignorare questo fatto, tanto sono riuscito a compensare
            //       questo problema con il freeSpaceFactor

            // NOTA2: Al limite, potrei provare a fare un test anche su di un numero a meta'
            //        della finestra logica, ma rischio che abbia un numero di cifre troppo elevato
            //        rispetto alle cifre effettivamente stampate

            // Trovo la dimensione in pixel del numero piu' grande che devo visualizzare
            float startValue = 0;
            float stopValue = 0;
            float maxNumberWidth = 0;

            // Valore all'inizio e alla fine del righello
            if (horizontalRuler)
            {
                startValue = LogicalOrigin.X;
                stopValue = LogicalOrigin.X + LogicalWidth;
            }
            else
            {
                startValue = LogicalOrigin.Y;
                stopValue = LogicalOrigin.Y + LogicalHeight;
            }

            // Aggiorno il valore in modo da tener conto della misura in cui sto visualizzando i dati
            // NOTA: Come al solito, il valore di partenza e' in micron
            startValue /= MeasureSystem.CustomUnitToMicron(1, UnitOfMeasure);
            stopValue /= MeasureSystem.CustomUnitToMicron(1, UnitOfMeasure);


            // Dimensione del numero all'inizio e numero alla fine del righello
            startValue = digitMaskCreator.MaskWidth(startValue);
            stopValue = digitMaskCreator.MaskWidth(stopValue);

            // Massima dimensione del numero da scrivere [pixel]
            maxNumberWidth = Math.Max(startValue, stopValue);

            // Spazio disponibile [pixel]
            int availableSpace = 0;
            if (horizontalRuler)
            {
                availableSpace = Convert.ToInt32(PictureBoxControl.GraphicInfo.ToPhysicalDimension(LogicalWidth));
            }
            else
            {
                availableSpace = Convert.ToInt32(PictureBoxControl.GraphicInfo.ToPhysicalDimension(LogicalHeight));
            }

            // Numero di quote che si suppone che vengano visualizzate nel righello
            int totalNumQuotes = availableSpace / Convert.ToInt32(maxNumberWidth);

            // Check se ho un numero di quote valido
            if (totalNumQuotes == 0)
            {
                return 0;
            }

            // Trovo il passo di base con cui scrivero' i numeri nel righello [micron]
            if (horizontalRuler)
            {
                return LogicalWidth / totalNumQuotes;
            }
            else
            {
                return LogicalHeight / totalNumQuotes;
            }
        }

        /// <summary>
        /// Ridisegna la bitmap che rappresenta il righello orizzontale
        /// </summary>
        private Bitmap RedrawHorizontalRuler()
        {
            Graphics GR = null;
            try
            {
                // Check se questo righello ha dimensioni valide
                if (Width <= 0)
                {
                    return null;
                }

                // Imposto il Graphics su cui disegnare e la dimensione logica del righello
                // NOTA: La dimensione logica deve essere un pixel di meno rispetto alla dimensione fisica della bitmap,
                //       altrimenti la linea che vado a disegnare sul contorno interno non risulta visibile.
                // NOTA: Il fatto di diminuire queso valore non influlenza la posizione delle tacche del righello, ma solo la loro "altezza"
                GR = PictureBoxControl.GetScaledGraphicObject(myHRulerBmp);
                int rulerLogicSize = (int)((Size - 1) / ScaleFactor);

                // Disegno lo sfondo del righello
                GR.Clear(RulerColor);


                // Disegno il contorno del righello
                // NOTA: Non devo disegnare un rettangolo completo attorno al righello, perhce' altrimenti il bordo rivolto 
                //       verso l'esterno mi da' un brutto effetto ottico quando la PictureBox e' in modalita' BorderStyle.Simple.
                //       Quindi disegno solo una riga nera rivolta verso l'interno della PictureBox

                GR.DrawLine(RulerPen, LogicalOrigin.X + rulerLogicSize, LogicalOrigin.Y + rulerLogicSize, LogicalOrigin.X + LogicalWidth, LogicalOrigin.Y + rulerLogicSize);

                // Fattore moltiplicativo necessario per disegnare il giusto numero sulla quota
                float RulerValueFactor = (float)(1.0 / MeasureSystem.CustomUnitToMicron(1, UnitOfMeasure));


                // Calcolo il passo del righello
                float rulerStep = GetRulerStep();
                if ((rulerStep <= 0))
                {
                    return myHRulerBmp;
                }


                int XDisplacement = 0;
                int OverNeedles = 0;

                XDisplacement = 0;
                OverNeedles = 1;


                // Trovo il primo punto da disegnare che non cade sotto al righello verticale
                float startPoint = (float)Math.Ceiling((LogicalOrigin.X + rulerLogicSize) / rulerStep) * rulerStep + XDisplacement;

                // Valori della Y che restano fissi: per disegnare il numero, la linea alta 1/2 righello,
                // la linea alta 1/4 di righello, la linea di base del righello
                int yCoord = (int)(LogicalOrigin.Y + 2 / ScaleFactor);
                int yHalfLine = LogicalOrigin.Y + rulerLogicSize / 2;
                int yQuarterLine = LogicalOrigin.Y + rulerLogicSize - rulerLogicSize / 4;
                int yRulerBase = LogicalOrigin.Y + rulerLogicSize;


                // Disegno l'interno del righello
                for (float xCoord = startPoint; xCoord <= LogicalOrigin.X + LogicalWidth; xCoord += rulerStep)
                {
                    // Disegno le due lineette verticali
                    GR.DrawLine(RulerPen, xCoord, yHalfLine, xCoord, yRulerBase);
                    GR.DrawLine(RulerPen, Convert.ToInt32(xCoord + rulerStep / 2), yQuarterLine, Convert.ToInt32(xCoord + rulerStep / 2), yRulerBase);
                    // Disegno la quota sul righello
                    //GR.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    digitMaskCreator.DrawScaledNumber(GR, (xCoord - XDisplacement) * RulerValueFactor - OverNeedles + 1, xCoord, yCoord, ScaleFactor, true);
                    //GR.SmoothingMode = Drawing2D.SmoothingMode.None
                }

                // Il righello orizzontale e' aggiornato
                NeedsHorizontalRedraw = false;

                return myHRulerBmp;
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                return null;
            }
            finally
            {
                // Libero la memoria allocata per l'oggetto Graphics
                if (GR != null)
                {
                    GR.Dispose();
                }
            }
        }

        /// <summary>
        /// Ridisegna la bitmap che rappresenta il righello verticale
        /// </summary>
        private Bitmap RedrawVerticalRuler()
        {
            Graphics GR = null;
            try
            {
                // Check se questo righello ha dimensioni valide
                if (Height <= 0)
                {
                    return null;
                }

                // Imposto il Graphics su cui disegnare e la dimensione logica del righello
                // NOTA: La dimensione logica deve essere un pixel di meno rispetto alla dimensione fisica della bitmap,
                //       altrimenti la linea che vado a disegnare sul contorno interno non risulta visibile.
                // NOTA: Il fatto di diminuire queso valore non influlenza la posizione delle tacche del righello, ma solo la loro "altezza"
                GR = PictureBoxControl.GetScaledGraphicObject(myVRulerBmp);
                int rulerLogicSize = (int)((Size - 1) / ScaleFactor);

                // Disegno lo sfondo del righello
                GR.Clear(RulerColor);

                // Disegno il contorno del righello
                // NOTA: Non devo disegnare un rettangolo completo attorno al righello, perhce' altrimenti il bordo rivolto 
                //       verso l'esterno mi da' un brutto effetto ottico quando la PictureBox e' in modalita' BorderStyle.Simple.
                //       Quindi disegno solo una riga nera rivolta verso l'interno della PictureBox

                GR.DrawLine(RulerPen, LogicalOrigin.X + rulerLogicSize, LogicalOrigin.Y + rulerLogicSize, LogicalOrigin.X + rulerLogicSize, LogicalOrigin.Y + LogicalHeight);

                // Fattore moltiplicativo necessario per disegnare il giusto numero sulla quota
                float RulerValueFactor = (float)(1.0 / MeasureSystem.CustomUnitToMicron(1, UnitOfMeasure));

                // Calcolo il passo del righello
                float rulerStep = GetRulerStep();

                // Check se ho un passo valido
                if ((rulerStep <= 0))
                {
                    return myVRulerBmp;
                }

                // Trovo il primo punto da disegnare che non cade sotto al righello orizzontale
                int startPoint = (int)(Math.Ceiling((LogicalOrigin.Y + rulerLogicSize) / rulerStep) * rulerStep);

                // Valori della X che restano fissi: per disegnare il numero, la linea alta 1/2 righello,
                // la linea alta 1/4 di righello, la linea di base del righello
                int xCoord = (int)(LogicalOrigin.X + 2 / ScaleFactor);
                int xHalfLine = LogicalOrigin.X + rulerLogicSize / 2;
                int xQuarterLine = LogicalOrigin.X + rulerLogicSize - rulerLogicSize / 4;
                int xRulerBase = LogicalOrigin.X + rulerLogicSize;

                // Disegno l'interno del righello
                for (float yCoord = startPoint; yCoord <= LogicalOrigin.Y + LogicalHeight; yCoord += rulerStep)
                {
                    // Disegno le due lineette orizzontali
                    GR.DrawLine(RulerPen, xHalfLine, yCoord, xRulerBase, yCoord);
                    GR.DrawLine(RulerPen, xQuarterLine, Convert.ToInt32(yCoord + rulerStep / 2), xRulerBase, Convert.ToInt32(yCoord + rulerStep / 2));
                    // Disegno la quota sul righello
                    //GR.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    digitMaskCreator.DrawScaledNumber(GR, yCoord * RulerValueFactor, xCoord, yCoord, ScaleFactor, false);
                    //GR.SmoothingMode = Drawing2D.SmoothingMode.None
                }

                // Il righello verticale e' aggiornato
                NeedsVerticalRedraw = false;

                return myVRulerBmp;
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                return null;
            }
            finally
            {
                // Libero la memoria allocata per l'oggetto Graphics
                if (GR != null)
                {
                    GR.Dispose();
                }
            }
        }

        /// <summary>
        /// Crea la penna da usare per le operazioni di drag and drop dei righelli
        /// e la assegna a myDragPen
        /// </summary>
        private void CreateDragPen()
        {
            myDragPen = new Pen(myDragLineColor);
            // Imposto il pattern che andro' ad usare per le linee di drag
            float[] DashPattern = {
                20,
                7,
                1,
                7
            };
            myDragPen.DashPattern = DashPattern;
            myDragPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
        }

        /// <summary>
        /// Controlla se la bitmap del righello orizzontale va ricreata oppure no.
        /// </summary>
        private void CheckHorizontalRulerBitmap()
        {
            // Se ho cambiato la dimensione del righello orizzontale, devo distruggere la bitmap esistente
            if ((myHRulerBmp != null) && (myHRulerBmp.Width != NeededBitmapWidth))
            {
                myHRulerBmp.Dispose();
                myHRulerBmp = null;
            }
            // Se e' necessario, creo la bitmap su cui disegnare
            if ((myHRulerBmp == null))
            {
                myHRulerBmp = new Bitmap(NeededBitmapWidth, NeededBitmapRulerSize);
                //MsgBox(String.Format("Rulers: Allocata una bitmap orizzontale da {0}x{1}", myHRulerBmp.Width, myHRulerBmp.Height))
                // Aggiorno il flag "necessario ridisegno"
                NeedsHorizontalRedraw = true;
            }
        }


        /// <summary>
        /// Controlla se la bitmap del righello verticale va ricreata oppure no.
        /// </summary>
        private void CheckVerticalRulerBitmap()
        {
            // Se ho cambiato la dimensione del righello verticale, devo distruggere la bitmap esistente
            if ((myVRulerBmp != null) && (myVRulerBmp.Height != NeededBitmapHeight))
            {
                myVRulerBmp.Dispose();
                myVRulerBmp = null;
            }
            // Se e' necessario, creo la bitmap su cui disegnare
            if ((myVRulerBmp == null))
            {
                myVRulerBmp = new Bitmap(NeededBitmapRulerSize, NeededBitmapHeight);
                //MsgBox(String.Format("Rulers: Allocata una bitmap verticale da {0}x{1}", myVRulerBmp.Width, myVRulerBmp.Height))
                // Aggiorno il flag "necessario ridisegno"
                NeedsVerticalRedraw = true;
            }
        }

        /// <summary>
        /// Evento generato quando cambia l'unita' di misura della PictureBox associata.
        /// </summary>
        private void PictureBox_MeasureUnitChanged(MeasureSystem.enUniMis unit)
        {
            // Quando cambia l'unita' di misura, ho bisogno di un ridisegno di entrambi i righelli
            NeedsHorizontalRedraw = true;
            NeedsVerticalRedraw = true;
        }

        /// <summary>
        /// Disegna i righelli sul Graphics passatogli.
        /// NOTA: Il graphics passato puo' essere stato scalato oppure no, per questa routine la cosa e' ininfluente.
        /// </summary>
        public void Draw(Graphics GR)
        {
            try
            {
                // Check se i righelli hanno dimensioni valide
                if ((Width <= 0) || (Height <= 0))
                {
                    return;
                }

                // Check se dall'ultimo ridisegno effettuato e' cambiata la View corrente o i dati di ridisegno delle bitmap
                // NOTA BENE: In questo caso l'operatore "<>" fa un confronto di ConversionInfo, non di GraphicInfo
                if ((myLastGraphicInfo == null) || (myLastGraphicInfo != PictureBoxControl.GraphicInfo))
                {
                    NeedsHorizontalRedraw = true;
                    NeedsVerticalRedraw = true;
                }

                // Controlla se le bitmap su cui disegnare i righelli vanno ricreate o aggiornate.
                // NOTA: Se le bitmap vengono modificate, imposta NeedsHorizontalRedraw e NeedsVerticalRedraw a true
                CheckHorizontalRulerBitmap();
                CheckVerticalRulerBitmap();

                // Se serve, ridisegno le bitmap dei righelli
                if (NeedsHorizontalRedraw)
                {
                    RedrawHorizontalRuler();
                }
                if (NeedsVerticalRedraw)
                {
                    RedrawVerticalRuler();
                }

                // Aggiorno i dati relativi all'ultimo ridisegno effettuato
                myLastGraphicInfo = (ConversionInfo)PictureBoxControl.GraphicInfo.Clone();


                // Check se tutte le bitmap sono ok
                bool bitmapOk = true;
                bitmapOk = bitmapOk && (myHRulerBmp != null) && (myHRulerBmp.Width > 0) && (myHRulerBmp.Height > 0);
                bitmapOk = bitmapOk && (myVRulerBmp != null) && (myVRulerBmp.Width > 0) && (myVRulerBmp.Height > 0);
                bitmapOk = bitmapOk && (myOriginBmp != null) && (myOriginBmp.Width > 0) && (myOriginBmp.Height > 0);
                bitmapOk = bitmapOk && (myOriginBmpSnapped != null) && (myOriginBmpSnapped.Width > 0) && (myOriginBmpSnapped.Height > 0);
                if (!bitmapOk)
                {
                    //Interaction.MsgBox("Found invalid bitmap in " + this.GetType.FullName() + ".Paint()");
                    return;
                }

                // Salvo lo stato precedente e cancello le eventuali trasformazioni
                GraphicsState oldState = GR.Save();
                GR.ResetTransform();

                try
                {
                    // Disegno i righelli
                    GR.DrawImageUnscaled(myHRulerBmp, 0, 0);
                    GR.DrawImageUnscaled(myVRulerBmp, 0, 0);

                    GR.DrawImage(myOriginBmp, 0, 0, myOriginBmp.Width, myOriginBmp.Height);
                }
                finally
                {
                    // Ripristino lo stato precedente
                    GR.Restore(oldState);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Disegna la linea di riferimento per il drag and drop dal righello orizzontale
        /// </summary>
        public void DrawHorizontalDragDropLine(Graphics GR, int Y)
        {
            try
            {
                float yCoord = (Y - LogicalOrigin.Y) * ScaleFactor;
                GR.DrawLine(myDragPen, 0, yCoord, Width, yCoord);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Disegna la linea di riferimento per il drag and drop dal righello verticale
        /// </summary>
        public void DrawVerticalDragDropLine(Graphics GR, int X)
        {
            try
            {
                float xCoord = (X - LogicalOrigin.X) * ScaleFactor;
                GR.DrawLine(myDragPen, xCoord, 0, xCoord, Height);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Ritorna il passo del righello orizzontale o di quello verticale
        /// </summary>
        public float GetRulerStep()
        {
            // Trovo il passo di base con cui scrivero' i numeri nel righello [micron]
            // Il passo di base e' dato dal passo maggiore necessario per i due righelli (orizzontale e verticale)
            double stepValue = Math.Max(CalculateBaseStep(true), CalculateBaseStep(false));

            // Check se ho un passo di base valido
            if (stepValue == 0)
            {
                return 1f;
            }

            // Faccio in modo che tra un numero e l'altro ci sia un po' di spazio libero
            stepValue *= FreeSpaceFactor;

            // Se non una unita' di misura metrica, devo ricordarmi che base numerica uso,
            // e adeguare il valore del passo alla base numerica in uso
            double baseUnit = 1;
            if (UnitOfMeasure == MeasureSystem.enUniMis.inches)
            {
                stepValue /= 25400;
                baseUnit = 25400;
            }

            // Riporto il passo ad un multiplo di 1, 2, 5, 10, 20, 50, ecc...
            double[] valuesArray = {
                1,
                2,
                5
            };

            // Riporto il passo ad un multiplo dei numeri presenti nell'array
            // Il numero scelto tra quelli presenti e' quello che mi minimizza l'errore
            double actualLog = 0;
            double actualRounded = 0;
            double actualError = 0;
            double bestStep = 0;
            double minError = double.MaxValue;
            foreach (double actualValue in valuesArray)
            {
                // Potenza di 10 che mi porta al passo attuale, in funzione del possibile step
                actualLog = Math.Log10(stepValue / actualValue);
                // Calcolo l'arrotondamento piu' vicino e l'errore associato a questo possibile step
                actualRounded = Math.Round(actualLog);
                actualError = Math.Abs(actualRounded - actualLog);
                // Scelgo lo step con errore minore
                if (actualError < minError)
                {
                    bestStep = actualValue * baseUnit * Math.Pow(10, actualRounded);
                    minError = actualError;
                }
            }
            return (float)bestStep;
        }
    }
}
