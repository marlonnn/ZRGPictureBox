using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Summer.System.Log;

namespace ZRGPictureBox
{
    public partial class ZRGPictureBoxControl : UserControl
    {
        public bool HScrollable
        {
            get
            {
                return this.HScroll;
            }
        }

        public bool VScrollable
        {
            get
            {
                return this.VScroll;
            }
        }
        #region "Pan & zoom"
        public const float ZoomMultiplier = 1.25f;
        public const float PanFactorNoShift = 100f / 3f;
        #endregion

        public const float PanFactorWithShift = 10f;

        //"Size"
        public static readonly Size DefaultMinLogicalWindowSize = new Size(2000, 2000);
        public static readonly Size DefaultMaxLogicalWindowSize = new Size(100000000, 100000000);

        /// <summary>
        /// Unita' di misura di deafult utilizzata per la visualizzazione delle coordinate e per i righelli
        /// </summary>

        public const MeasureSystem.enUniMis DefaultUnitOfMeasure = MeasureSystem.enUniMis.mm;

        #region "Eventi"
        public new event MouseClickEventHandler MouseClick;
        public delegate void MouseClickEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.MouseEventArgs e, Point LogicalCoord, enClickAction CurrentClickAction);
        public new event MouseMoveEventHandler MouseMove;
        public delegate void MouseMoveEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.MouseEventArgs e, Point LogicalCoord, enClickAction CurrentClickAction);
        public new event MouseDownEventHandler MouseDown;
        public delegate void MouseDownEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.MouseEventArgs e, Point LogicalCoord, enClickAction CurrentClickAction);
        public new event MouseUpEventHandler MouseUp;
        public delegate void MouseUpEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.MouseEventArgs e, Point LogicalCoord, enClickAction CurrentClickAction);
        public new event MouseEnterEventHandler MouseEnter;
        public delegate void MouseEnterEventHandler(ZRGPictureBoxControl sender, System.EventArgs e);
        public new event MouseLeaveEventHandler MouseLeave;
        public delegate void MouseLeaveEventHandler(ZRGPictureBoxControl sender, System.EventArgs e);
        public new event PaintEventHandler Paint;
        public delegate void PaintEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.PaintEventArgs e);

        public event OnMeasureCompletedEventHandler OnMeasureCompleted;
        public delegate void OnMeasureCompletedEventHandler(ZRGPictureBoxControl sender, Point StartPoint, Point EndPoint);
        public event OnRedrawCompletedEventHandler OnRedrawCompleted;
        public delegate void OnRedrawCompletedEventHandler(ZRGPictureBoxControl sender, bool CacheRebuilded);
        public event OnPictureBoxDoubleClickEventHandler OnPictureBoxDoubleClick;
        public delegate void OnPictureBoxDoubleClickEventHandler(ZRGPictureBoxControl sender, System.Windows.Forms.MouseEventArgs e, Point LogicalCoord);
        public event OnMinimumZoomLevelReachedEventHandler OnMinimumZoomLevelReached;
        public delegate void OnMinimumZoomLevelReachedEventHandler(ZRGPictureBoxControl sender);
        public event OnMaximumZoomLevelReachedEventHandler OnMaximumZoomLevelReached;
        public delegate void OnMaximumZoomLevelReachedEventHandler(ZRGPictureBoxControl sender);

        public event OnMeasureUnitChangedEventHandler OnMeasureUnitChanged;
        public delegate void OnMeasureUnitChangedEventHandler(MeasureSystem.enUniMis unit);
        public event OnClickActionChangedEventHandler OnClickActionChanged;
        public delegate void OnClickActionChangedEventHandler(enClickAction oldClickAction, enClickAction newClickAction);
        #endregion

        private ConversionInfo myGraphicInfo = new ConversionInfo();
        private enClickAction myClickAction = enClickAction.Zoom;
        private CoordinatesBox myCoordinatesBox;
        private Color myDefaultBackgroundColor = Color.WhiteSmoke;

        private Color myZoomSelectionBoxColor = Color.Black;
        private bool myShowGrid = true;
        private GridKind myGridView = GridKind.Crosses;
        private int myGridStep = 10000;

        private bool mySmartGridAdjust = true;

        ///' <summary>
        ///' Bitmap che costituisce il buffer video di primo livello (persistente fino al Refresh())
        ///' </summary>

        private Bitmap myRefreshBackBuffer;

        /// <summary>
        /// Bitmap che costituisce il buffer video di secondo livello (persistente fino al Redraw())
        /// </summary>

        private Bitmap myRedrawBackBuffer;

        private bool myIsDragging = false;
        private bool myIsLoaded = false;
        private Rulers myRulers;
        private Point myLastMouseDownPoint;
        public Point LastMouseDownLogicalCoord
        {
            get { return myLastMouseDownPoint; }
        }
        private DistanceRuler withEventsField_myDistanceRuler;
        private DistanceRuler myDistanceRuler
        {
            get { return withEventsField_myDistanceRuler; }
            set
            {
                if (withEventsField_myDistanceRuler != null)
                {
                    withEventsField_myDistanceRuler.CaptureFinished -= myDistanceRuler_CaptureFinished;
                }
                withEventsField_myDistanceRuler = value;
                if (withEventsField_myDistanceRuler != null)
                {
                    withEventsField_myDistanceRuler.CaptureFinished += myDistanceRuler_CaptureFinished;
                }
            }
        }

        private bool myIsLayoutSuspended = true;

        private RECT myLastVisibleAreaRequested = DefaultRect;
        private RECT myResizeBeginEndPreviewArea = DefaultRect;
        private ResizeMode myResizeMode = ResizeMode.Stretch;
        private bool myIsBetweenResizeBeginEnd = false;

        Rectangle myBeginResizeClientArea;

        private enBitmapOriginPosition myPictureBoxImagePosition = enBitmapOriginPosition.TopLeft;
        private Point myPictureBoxImageCustomOrigin;
        private bool myShowPictureBoxImage = true;
        private cBackImageGraphics myPictureBoxImageGR;
        private System.Drawing.Image myPictureBoxImage;
        private int myPictureBoxImagePixelSize_micron = 100;
        public int BackgroundImagePixelSize_Mic
        {
            get { return myPictureBoxImagePixelSize_micron; }
            set
            {
                if (myPictureBoxImagePixelSize_micron != value)
                {
                    myPictureBoxImagePixelSize_micron = value;
                    myPictureBoxImageGR = new cBackImageGraphics((Bitmap)myPictureBoxImage, ImageCustomOrigin.X, ImageCustomOrigin.Y, enBitmapOriginPosition.TopLeft, myPictureBoxImagePixelSize_micron, myPictureBoxImagePixelSize_micron);
                }
            }
        }

        public Point ImageCustomOrigin
        {
            get { return myPictureBoxImageCustomOrigin; }
            set { myPictureBoxImageCustomOrigin = value; }
        }
        public enBitmapOriginPosition ImagePosition
        {
            get { return myPictureBoxImagePosition; }
            set
            {
                myPictureBoxImagePosition = value;
                if (myPictureBoxImage != null)
                {
                    myPictureBoxImageGR = new cBackImageGraphics((Bitmap)myPictureBoxImage, ImageCustomOrigin.X, ImageCustomOrigin.Y, 
                        enBitmapOriginPosition.TopLeft, myPictureBoxImagePixelSize_micron, myPictureBoxImagePixelSize_micron);
                }
            }
        }
        public Image Image
        {
            get { return myPictureBoxImage; }
            set
            {
                myPictureBoxImage = value;
                if (value != null)
                {
                    myPictureBoxImageGR = new cBackImageGraphics((Bitmap)myPictureBoxImage, ImageCustomOrigin.X, ImageCustomOrigin.Y, 
                        enBitmapOriginPosition.TopLeft, myPictureBoxImagePixelSize_micron, myPictureBoxImagePixelSize_micron);
                }
                else
                {
                    myPictureBoxImageGR = null;
                }
            }
        }

        #region "Dimensioni e unita' di misura"
        private BorderStyle myBorderStyle = BorderStyle.FixedSingle;
        private MeasureSystem.enUniMis myUnitOfMeasure = DefaultUnitOfMeasure;
        private Size myMinLogicalWindowSize = DefaultMinLogicalWindowSize;
        #endregion
        private Size myMaxLogicalWindowSize = DefaultMaxLogicalWindowSize;

        #region "Flag per la visualizzazione"
        private bool myShowMouseCoordinates = true;
        private bool myShowRulers = true;
        #endregion
        private bool myIsChangingAutoScroll = false;

        #region "Box di selezione/zoom"
        #endregion
        private SelectionBoxElement mySelectionBox;

        protected override Size DefaultSize
        {
            get { return new Size(560, 400); }
        }
        public float ScaleFactor
        {
            get { return GraphicInfo.ScaleFactor; }
            set
            {
                // Se il nuovo valore mi porterebbe fuori dai limiti minimi o massimi, lo ignoro
                if ((Width / value) > MaxLogicalWindowSize.Width && value < GraphicInfo.ScaleFactor)
                    return;
                if ((Height / value) > MaxLogicalWindowSize.Height && value < GraphicInfo.ScaleFactor)
                    return;
                if ((Width / value) < MinLogicalWindowSize.Width && value > GraphicInfo.ScaleFactor)
                    return;
                if ((Height / value) < MinLogicalWindowSize.Height && value > GraphicInfo.ScaleFactor)
                    return;
                // Aggiorno i dati interni
                GraphicInfo.ScaleFactor = value;
            }
        }

        public System.Drawing.Point LogicalOrigin
        {
            get { return GraphicInfo.LogicalOrigin; }
            set { GraphicInfo.LogicalOrigin = value; }
        }

        public Point LogicalCenter
        {
            get { return new Point(LogicalOrigin.X + LogicalWidth / 2, LogicalOrigin.Y + LogicalHeight / 2); }
        }

        public int LogicalWidth
        {
            get { return GraphicInfo.LogicalWidth; }
            set { GraphicInfo.LogicalWidth = value; }
        }

        public int LogicalHeight
        {
            get { return GraphicInfo.LogicalHeight; }
            set { GraphicInfo.LogicalHeight = value; }
        }

        public RECT LogicalArea
        {
            get { return GraphicInfo.LogicalArea; }
            private set { GraphicInfo.LogicalArea = value; }
        }
        public Size MinLogicalWindowSize
        {
            get { return myMinLogicalWindowSize; }
            set { myMinLogicalWindowSize = value; }
        }
        public Size MaxLogicalWindowSize
        {
            get { return myMaxLogicalWindowSize; }
            set
            {
                myMaxLogicalWindowSize = value;
                // Aggiorno i dati delle scrollbar
                if (ShowScrollbars)
                {
                    UpdateScrollbars();
                }
            }
        }

        public ConversionInfo GraphicInfo
        {
            get { return myGraphicInfo; }
            private set { myGraphicInfo = value; }
        }

        #region "flag for display"
        /// <summary>
        /// displays the background image of the picturebox.
        /// </summary>
        [Description("displays the background image of the picturebox."), Category("display options"), DefaultValue(true)]
        public bool ShowPictureBoxBackgroundImage
        {
            get { return myShowPictureBoxImage; }
            set
            {
                // Check se il valore e' gia' quello desiderato
                if (myShowPictureBoxImage == value)
                {
                    return;
                }
                myShowPictureBoxImage = value;
            }
        }

        /// <summary>
        /// allows to display the coordinates where the mouse is
        /// </summary>
        [Description("allows to display the coordinates where the mouse is"), DefaultValue(true)]
        public bool ShowMouseCoordinates
        {
            get { return myShowMouseCoordinates; }
            set { myShowMouseCoordinates = value; }
        }

        [Description("allows to display the grid"), Category("display options"), DefaultValue(true)]
        public bool ShowGrid
        {
            get { return myShowGrid; }
            set { myShowGrid = value; }
        }
        /// <summary>
        /// allows to display the rulers
        /// </summary>
        [Description("allows to display the rulers"), Category("display options"), DefaultValue(true)]
        public bool ShowRulers
        {
            get { return myShowRulers; }
            set { myShowRulers = value; }
        }
        #endregion

        #region "Gestione del double buffering video"

        ///' <summary>
        ///' bitmap which is the first level buffer level (persistent until the refresh ())
        ///' </summary>
        protected Bitmap RefreshBackBuffer
        {
            get { return myRefreshBackBuffer; }
        }

        /// <summary>
        /// bitmap which is the second level buffer level (persistent until redraw ())
        /// </summary>
        protected Bitmap RedrawBackBuffer
        {
            get { return myRedrawBackBuffer; }
        }

        #endregion

        #region "elements of the picturebox"
        public static Color AxesColor
        {
            get { return Color.Navy; }
        }
        public static Color RulerColor
        {
            get { return Color.White; }
        }
        public Color BackgroundColor
        {
            get { return myDefaultBackgroundColor; }
            set { myDefaultBackgroundColor = value; }
        }
        /// <summary>
        /// sets the color of the box of zoom / selection
        /// </summary>
        [Description("sets the color of the box of zoom / selection"), Category("Colors"), DefaultValue(typeof(Color), "Black")]
        public Color CrossCursorColor
        {
            get { return FullCrossCursor.Color; }
            set { FullCrossCursor.Color = value; }
        }
        public Color GridColor
        {
            get { return Color.LightSteelBlue; }
        }
        public Color SnapGridColor
        {
            get { return Color.Gray; }
        }
        /// <summary>
        /// sets the color of the box of zoom / selection
        /// </summary>
        [Description("sets the color of the box of zoom / selection"), Category("Colors"), DefaultValue(typeof(Color), "Black")]
        public Color ZoomSelectionBoxColor
        {
            get { return myZoomSelectionBoxColor; }
            set
            {
                try
                {
                    myZoomSelectionBoxColor = value;
                }
                catch (Exception ex)
                {
                    //Interaction.MsgBox(ex.Message);
                    LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                    LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                }
            }
        }
        #endregion

        #region "grid and snap"
        [Description("set the kind of display grids"), DefaultValue(typeof(GridKind), "Crosses")]
        public GridKind GridView
        {
            get { return myGridView; }
            set { myGridView = value; }
        }
        /// <summary>
        /// set the step of the grid
        /// </summary>
        [Description("set the step of the grid"), DefaultValue(10000)]
        public int GridStep
        {
            get { return myGridStep; }
            set { myGridStep = value; }
        }
        public bool SmartGridAdjust
        {
            get { return mySmartGridAdjust; }
            set { mySmartGridAdjust = value; }
        }
        #endregion

        #region "Tasti premuti"

        /// <summary>
        /// returns true if the shift key is pressed
        /// </summary>
        public static bool IsShiftKeyPressed
        {
            get { return (Control.ModifierKeys & Keys.Shift) != 0; }
        }
        public static bool IsAltKeyPressed
        {
            get { return (Control.ModifierKeys & Keys.Alt) != 0; }
        }
        public static bool IsCtrlKeyPressed
        {
            get { return (Control.ModifierKeys & Keys.Control) != 0; }
        }
        #endregion

        #region "Misura delle distanze"
        private void myDistanceRuler_CaptureFinished(object sender, CaptureEventArgs e)
        {
            if (OnMeasureCompleted != null)
            {
                OnMeasureCompleted(this, GraphicInfo.ToLogicalPoint(e.StartPoint), GraphicInfo.ToLogicalPoint(e.EndPoint));
            }
        }
        #endregion

        #region "context menu"
        public event ShowContextMenuRequiredEventHandler ShowContextMenuRequired;
        public delegate void ShowContextMenuRequiredEventHandler(ZRGPictureBoxControl sender, float X, float Y);
        public void RaiseContextMenuRequest(ZRGPictureBoxControl sender, float X, float Y)
        {
            try
            {
                if (ShowContextMenuRequired != null)
                {
                    ShowContextMenuRequired(sender, X, Y);
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region "Cursori"
        /// <summary>
        /// Cursore correntemente visualizzato
        /// </summary>
        protected Cursor CurrentCursor
        {
            get { return base.Cursor; }
            set
            {
                // Se mi e' stato richiesto il cursore di waiting, non permetto cambiamenti
                if (UseWaitCursor)
                {
                    return;
                }
                // Altrimenti aggiorno il cursore visualizzato
                // NOTA: Devo modificare MyBase.Cursor, non Me.Cursor, altrimenti entro in un ciclo infinito
                //       (ed inoltre perdo la reference al cursore di default)
                if (!object.ReferenceEquals(base.Cursor, value))
                {
                    base.Cursor = value;
                }
            }
        }

        /// <summary>
        /// Cursore di default
        /// </summary>
        protected override Cursor DefaultCursor
        {
            get
            {
                if (this.ClickAction == enClickAction.Zoom)
                {
                    return cCommonCursors.ZoomCursor;
                }
                else
                {
                    return cCommonCursors.EditCursor;
                }
            }
        }
        #endregion

        #region "Scrollbar"

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Size AutoScrollMinSize
        {
            get { return base.AutoScrollMinSize; }
            private set { base.AutoScrollMinSize = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Size AutoScrollMargin
        {
            get { return base.AutoScrollMargin; }
            private set { base.AutoScrollMargin = value; }
        }

        /// <summary>
        /// allows you to display the scrollbar
        /// </summary>
        [Description("allows you to display the scrollbar"), DefaultValue(false)]
        public bool ShowScrollbars
        {
            get { return AutoScroll; }
            set
            {

                if (AutoScroll == value)
                {
                    return;
                }

                if (value)
                {
                    UpdateScrollbars();
                }

                myIsChangingAutoScroll = true;
                AutoScroll = value;
                myIsChangingAutoScroll = false;
            }
        }

        #endregion

        #region "Stato attuale"
        /// <summary>
        /// sets the type of action to be carried out on the click of a mouse
        /// </summary>
        [Description("sets the type of action to be carried out on the click of a mouse"), DefaultValue(typeof(enClickAction), "SelectObjects")]
        public enClickAction ClickAction
        {
            get { return myClickAction; }
            set
            {
                // Check se ho gia' il valore desiderato
                //If myClickAction = Value Then
                //    Exit Property
                //End If
                // Aggiorno i dati interni
                enClickAction oldClickAction = myClickAction;
                myClickAction = value;

                // Se sono in modalita' di progettazione, non devo fare altro
                if (DesignMode)
                {
                    return;
                }

                // Genero l'evento "modificato il tipo di azione da eseguire sul click del mouse"
                if (OnClickActionChanged != null)
                {
                    OnClickActionChanged(oldClickAction, myClickAction);
                }

                // NOTA: Se imposto la modalita' a Zoom, nel Designer sparisce il cursore.
                //       Penso che questo accada perche' il Designer non sa gestire la classe PictureBoxCursors. -> INVESTIGARE
                switch (myClickAction)
                {
                    case enClickAction.Zoom:
                        // L'eventuale box di selezione deve mantenere l'aspetto della finestra
                        SelectionBox.KeepAspectRatio = true;
                        Cursor = cCommonCursors.ZoomCursor;
                        break;
                    case enClickAction.MeasureDistance:
                        // L'eventuale box di selezione deve mantenere l'aspetto della finestra
                        SelectionBox.KeepAspectRatio = true;
                        Cursor = cCommonCursors.EditCursor;
                        break;
                }
            }
        }
        #endregion

        #region "Box di selezione/zoom"
        public SelectionBoxElement SelectionBox
        {
            get { return mySelectionBox; }
        }

        #endregion

        #region "variables for the Resize()"
        [DefaultValue(typeof(ResizeMode), "Stretch")]
        public ResizeMode ResizeMode
        {
            get { return myResizeMode; }
            set { myResizeMode = value; }
        }
        #endregion

        [Browsable(false)]
        public bool IsLayoutSuspended
        {
            get { return myIsLayoutSuspended; }
        }

        protected bool IsDragging
        {
            get { return myIsDragging; }
        }

        protected bool IsLoaded
        {
            get { return myIsLoaded; }
            private set { myIsLoaded = value; }
        }

        public bool ContainsMousePosition
        {
            //note: can not use the clientrectangle percvhe if the scrollbar are active.
            //the area occupied by the scrollbar is excluded from the area client
            //Dim p As Point = PointToClient(MousePosition)
            //If (p.X < 0) OrElse (p.X > Me.Size.Width) Then Return False
            //If (p.Y < 0) OrElse (p.Y > Me.Size.Height) Then Return False
            //Return True
            get { return this.ClientRectangle.Contains(PointToClient(MousePosition)); }
        }
        private CrossCursor myFullCrossCursor;
        private CrossCursor FullCrossCursor
        {
            get
            {
                if (myFullCrossCursor == null)
                {
                    myFullCrossCursor = new CrossCursor(this);
                }
                return myFullCrossCursor;
            }
        }
        private bool FullPictureBoxCross
        {
            get { return myClickAction == enClickAction.MeasureDistance; }
        }

        /// <summary>
        /// Unit di misura del sistema di coordinate logico della PictureBox
        /// </summary>
        [Description("Imposta l'unit di misura del sistema di coordinate logico della PictureBox"), DefaultValue(typeof(MeasureSystem.enUniMis), "Millimeter")]
        public MeasureSystem.enUniMis UnitOfMeasure
        {
            get { return myUnitOfMeasure; }
            set
            {
                if (myUnitOfMeasure == value)
                {
                    return;
                }
                myUnitOfMeasure = value;
                if (OnMeasureUnitChanged != null)
                {
                    OnMeasureUnitChanged(value);
                }
                // Se sto disegnando qualcosa che richiede l'unita' di misura, devo fare un ridisegno
                if (ShowMouseCoordinates || ShowRulers)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// returns the currently displayed by the picturebox coordinates area [logical]
        /// note: the area varies in the range of the zoom lens
        /// </summary>
        [Browsable(false)]
        public RECT VisibleRect
        {
            get { return new RECT(LogicalOrigin.X, LogicalOrigin.Y, LogicalOrigin.X + LogicalWidth, LogicalOrigin.Y + LogicalHeight); }
        }

        public ZRGPictureBoxControl(bool visible) : base()
        {
            myCoordinatesBox = new CoordinatesBox(this);
            myRulers = new Rulers(this);
            withEventsField_myDistanceRuler = new DistanceRuler(this);
            mySelectionBox = new SelectionBoxElement(this);
            Load += PictureBoxEx_Load;
            GiveFeedback += ListDragSource_GiveFeedback;
            //This call is required by the Windows Form Designer.

            try
            {
                this.Visible = visible;
                InitializeComponent();
                //Add any initialization after the InitializeComponent() call
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }

        public ZRGPictureBoxControl() : this(true)
        {
        }

        #region "Funzioni di supporto per gli eventi"

        /// <summary>
        /// Cancella gli eventuali dati temporanei usati tra un MouseDown e un MouseUp
        /// </summary>
        private void ResetTemporaryData()
        {
            // Cancello il rettangolo di selezione, in modo che non venga piu' ridisegnato
            SelectionBox.Reset();
        }

        /// <summary>
        /// Aggiorna lo stato del flag che indica se si sta facendo un drag and drop
        /// </summary>
        protected void UpdateDraggingState()
        {
            // Si puo' avere un drag and drop solo se il tasto sinistro e' abbassato
            // e se esiste un punto di partenza da cui si e' cominciato il drag
            // e se sono in standardView
            myIsDragging = (MouseButtons == MouseButtons.Left) && (!(SelectionBox.TopLeftCorner == RECT.InvalidPoint()));
            if (myIsDragging)
            {
                // Si puo' avere un drag and drop solo se mi sono spostato dal punto in cui avevo premuto il tasto
                Point physicalMousePos = PointToClient(MousePosition);
                int distanceX = physicalMousePos.X - myLastMouseDownPoint.X;
                int distanceY = physicalMousePos.Y - myLastMouseDownPoint.Y;
                myIsDragging = (Math.Abs(distanceX) >= 3 || Math.Abs(distanceY) >= 3);
            }
        }

        /// <summary>
        /// Aggiorna le dimensioni degli elementi che hanno bisogno di essere ridimensionati
        /// Crea delle nuove bitmap di primo e secondo livello in funzione delle nuove dimensioni.
        /// Ritorna true se le bitmap sono state aggiornate, false altrimenti.
        /// </summary>
        private bool UpdateDimensions()
        {
            // Se sono in fase di creazione del controllo o se ho sospeso il layout del controllo, posso uscire direttamente
            try
            {
                //If (Not Me.Created) OrElse IsLayoutSuspended Then
                //    Return False
                //End If
                // Check se la finestra e' minimizzata
                if ((Width < 1) || (Height < 1))
                {
                    return false;
                }

                // Per evitare di allocare e deallocare continuamente delle bitmap quando l'utente ridimensiona la finestra
                // trascinando con il mouse, le bitmap vengono allocate arrotondando ai 100 pixel superiori
                int newWidth = (int)(Math.Ceiling(Convert.ToDouble(this.Width) / 100.0) * 100);
                int newHeight = (int)(Math.Ceiling(Convert.ToDouble(this.Height) / 100.0) * 100);

                // Flag che indica se le bitmap vanno ricreate oppure no
                bool bitmapCreationNeeded = (myRedrawBackBuffer == null) || (myRedrawBackBuffer.Width < newWidth) || (myRedrawBackBuffer.Height < newHeight);
                if (bitmapCreationNeeded)
                {
                    // Pulisco eventuali bitmap precedenti
                    if ((myRedrawBackBuffer != null))
                        myRedrawBackBuffer.Dispose();
                    if ((myRefreshBackBuffer != null))
                        myRefreshBackBuffer.Dispose();
                    // Aggiorno le dimensioni delle bitmap su cui andro' a tracciare
                    myRedrawBackBuffer = new Bitmap(newWidth, newHeight);
                    myRefreshBackBuffer = new Bitmap(newWidth, newHeight);
                }

                // Ritorno true se le bitmap sono state aggiornate, false altrimenti.
                return bitmapCreationNeeded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region "Gestione degli eventi"

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                // Aggiorno lo stato del flag che indica se si sta facendo un drag and drop
                myIsDragging = false;

                // Posizione del mouse in vari tipi di coordinate
                Point logicalMousePos = GraphicInfo.ToLogicalPoint(e.X, e.Y);
                myLastMouseDownPoint = logicalMousePos;

                if (e.Button == MouseButtons.Right)
                {
                    if (MouseDown != null)
                    {
                        MouseDown(this, e, logicalMousePos, myClickAction);
                    }
                    base.OnMouseDown(e);
                    RaiseContextMenuRequest(this, e.X, e.Y);
                    return;
                }

                // Imposto il primo punto del rettangolo di selezione/zoom
                if ((e.Button == MouseButtons.Left))
                {
                    SelectionBox.TopLeftCorner = logicalMousePos;
                    SelectionBox.BottomRightCorner = logicalMousePos;
                }

                // Il tipo di azione da eseguire dipende dalla modalita' di click attuale
                switch (ClickAction)
                {
                    case enClickAction.None:
                        break; // TODO: might not be correct. Was : Exit Select
                    case enClickAction.MeasureDistance:
                        // Imposto il primo punto del righello usato per misurare
                        myDistanceRuler.MouseDown(this, e);
                        break;
                    case enClickAction.Zoom:
                        // L'eventuale box di selezione mantiene l'aspetto della finestra
                        SelectionBox.KeepAspectRatio = true;
                        break;
                }

                if (MouseDown != null)
                {
                    MouseDown(this, e, logicalMousePos, myClickAction);
                }
                base.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }
        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (IsDragging)
                {
                    return;
                }
                // Posizione del mouse in vari tipi di coordinate
                Point physicalMousePos = new Point(e.X, e.Y);
                Point logicalMousePos = GraphicInfo.ToLogicalPoint(physicalMousePos);
                // Genero l'evento 
                if (MouseClick != null)
                {
                    MouseClick(this, e, logicalMousePos, myClickAction);
                }
                base.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }


        private void ListDragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // Set the custom cursor based upon the effect.
            if (((e.Effect & DragDropEffects.Move) == DragDropEffects.Move))
            {
                Cursor.Current = Cursors.SizeAll;
            }
            else
            {
                Cursor.Current = Cursors.No;
            }

        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            bool needsRepaint = false;
            try
            {
                Point physicalMousePos = new Point(e.X, e.Y);
                Point logicalMousePos = GraphicInfo.ToLogicalPoint(physicalMousePos);

                // Aggiorno lo stato del flag che indica se si sta facendo un drag and drop
                this.UpdateDraggingState();

                // Il tipo di azione da eseguire dipende dalla modalita' di click attuale
                switch (myClickAction)
                {
                    case enClickAction.None:
                        break; // TODO: might not be correct. Was : Exit Select
                    case enClickAction.MeasureDistance:
                        // Se mi sto muovendo con il tasto sinistro premuto, aggiorno il righello usato per misurare
                        if (e.Button == System.Windows.Forms.MouseButtons.Left)
                        {
                            myDistanceRuler.MouseMove(this, e);
                            needsRepaint = true;
                        }
                        break;
                    case enClickAction.Zoom:
                        // Se mi sto muovendo con il tasto sinistro premuto,
                        // aggiorno il secondo punto del rettangolo di selezione
                        if (IsDragging)
                        {
                            SelectionBox.BottomRightCorner = logicalMousePos;
                            needsRepaint = true;
                        }
                        break;
                }

                MouseMove?.Invoke(this, e, logicalMousePos, myClickAction);
                base.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
            finally
            {
                // Se serve, ridisegno la finestra
                if (needsRepaint || myShowMouseCoordinates)
                {
                    Invalidate();
                }
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                // Posizione del mouse in vari tipi di coordinate
                Point logicalMousePos = GraphicInfo.ToLogicalPoint(e.X, e.Y);

                // Aggiorno il secondo punto del rettangolo di selezione
                if ((e.Button == MouseButtons.Left))
                {
                    if ((SelectionBox.BottomRightCorner != logicalMousePos))
                    {
                        SelectionBox.BottomRightCorner = logicalMousePos;
                    }
                }

                // Il tipo di azione da eseguire dipende dalla modalita' di click attuale
                switch (myClickAction)
                {
                    case enClickAction.None:
                        break; // TODO: might not be correct. Was : Exit Select
                    case enClickAction.MeasureDistance:
                        myDistanceRuler.MouseUp(this, e);
                        break;
                    case enClickAction.Zoom:
                        if (!IsDragging)
                        {
                            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                                ZoomForward(ref logicalMousePos);
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                                ZoomBack(ref logicalMousePos);
                        }
                        else
                        {
                            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                            {
                                ShowLogicalWindow(SelectionBox);
                            }
                        }
                        break;
                }

                if (MouseUp != null)
                {
                    MouseUp(this, e, logicalMousePos, myClickAction);
                }
                base.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
            finally
            {
                ResetTemporaryData();
            }
        }
        protected override void OnMouseDoubleClick(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                base.OnMouseDoubleClick(e);
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }
        private void PictureBoxEx_Load(object sender, System.EventArgs e)
        {
            // Inizializzo i dati interni
            Initialize();
            // Notifico che questo controllo e' stato caricato nel suo contenitore
            IsLoaded = true;
            // NOTA: Non serve fare un ridisegno, tanto nella Initialize() faccio uno zoom al rettangolo di default
            UpdateDimensions();
            if (DesignMode)
            {
                Redraw();
            }
        }

        public new void Invalidate()
        {
            base.Invalidate();
        }

        #endregion

        #region "Gestione delle scrollbar"

        /// <summary>
        /// Imposta il valore delle scrollbar (dipende dal livello di zoom della finestra logica)
        /// </summary>
        private void UpdateScrollbars()
        {
            // NOTA: La thumb di una scrollbar e' la parte che posso cliccare e trascinare qua e la

            // Aggiorno la dimensione che fa apparire le scrollbar, in pratica la modifico in modo
            // che la thumb delle scrollbar diventi piu' stretta o piu' larga in funzione dell'area logica visualizzata
            int newValueX = 0;
            int newValueY = 0;
            newValueX = (int)(Math.BigMul(this.Size.Width, MaxLogicalWindowSize.Width) / LogicalWidth);
            newValueY = (int)(Math.BigMul(this.Size.Height, MaxLogicalWindowSize.Height) / LogicalHeight);
            // Check se ho dei valori validi
            newValueX = Math.Max(newValueX, 0);
            newValueX = Math.Max(newValueY, 0);
            this.AutoScrollMinSize = new Size(newValueX, newValueY);

            // Aggiorno la posizione delle thumb all'interno delle scrollbar
            // NOTA: Devono essere sempre maggiori o uguali a zero
            newValueX = (int)(Math.BigMul(this.Size.Width, LogicalCenter.X + MaxLogicalWindowSize.Width / 2) / LogicalWidth);
            newValueY = (int)(Math.BigMul(this.Size.Height, LogicalCenter.Y + MaxLogicalWindowSize.Height / 2) / LogicalHeight);
            // Check se ho dei valori validi
            newValueX = Math.Max(newValueX, 0);
            newValueX = Math.Max(newValueY, 0);
            this.AutoScrollPosition = new Point(newValueX, newValueY);

            // Aggiorno gli spostamenti relativi alle scrollbar
            this.HorizontalScroll.SmallChange = (int)(Convert.ToSingle(LogicalWidth) * PanFactorWithShift / 100f);
            this.HorizontalScroll.LargeChange = (int)(Convert.ToSingle(LogicalWidth) * PanFactorNoShift / 100f);
            this.VerticalScroll.SmallChange = (int)(Convert.ToSingle(LogicalHeight) * PanFactorWithShift / 100f);
            this.VerticalScroll.LargeChange = (int)(Convert.ToSingle(LogicalHeight) * PanFactorNoShift / 100f);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            // In funzione del tipo di scroll, ho uno spostamento diverso 
            if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                //MsgBox("OnScroll: Horizontal")
                switch (se.Type)
                {
                    case ScrollEventType.SmallIncrement:
                        PanRight(PanFactorWithShift);
                        break;
                    case ScrollEventType.SmallDecrement:
                        PanLeft(PanFactorWithShift);
                        break;
                    case ScrollEventType.LargeIncrement:
                        PanRight(PanFactorNoShift);
                        break;
                    case ScrollEventType.LargeDecrement:
                        PanLeft(PanFactorNoShift);
                        break;
                    default:
                        // Arrivo qui se:
                        //  - l'utente sta trascinando la scrollbar (ScrollEventType.ThumbTrack)
                        //  - l'utente ha finito il trascinamento della scrollbar (ScrollEventType.ThumbPosition)
                        //  - la scrollbar e' stata portata al suo valore minimo o massimo (ScrollEventType.First, ScrollEventType.Last)
                        //  - (???) (ScrollEventType.EndScroll)
                        int newValueX = (int)(Math.BigMul(this.HorizontalScroll.Value, LogicalWidth) / this.Size.Width);
                        newValueX -= MaxLogicalWindowSize.Width / 2;
                        LogicalOrigin = new Point(newValueX, LogicalOrigin.Y);
                        Redraw();
                        break;
                        //MsgBox(se.Type.ToString())
                }
            }
            else
            {
                switch (se.Type)
                {
                    case ScrollEventType.SmallIncrement:
                        PanDown(PanFactorWithShift);
                        break;
                    case ScrollEventType.SmallDecrement:
                        PanUp(PanFactorWithShift);
                        break;
                    case ScrollEventType.LargeIncrement:
                        PanDown(PanFactorNoShift);
                        break;
                    case ScrollEventType.LargeDecrement:
                        PanUp(PanFactorNoShift);
                        break;
                    default:
                        // Arrivo qui se:
                        //  - l'utente sta trascinando la scrollbar (ScrollEventType.ThumbTrack)
                        //  - l'utente ha finito il trascinamento della scrollbar (ScrollEventType.ThumbPosition)
                        //  - la scrollbar e' stata portata al suo valore minimo o massimo (ScrollEventType.First, ScrollEventType.Last)
                        //  - (???) (ScrollEventType.EndScroll)
                        int newValueY = (int)(Math.BigMul(this.VerticalScroll.Value, LogicalHeight) / this.Size.Height);
                        newValueY -= MaxLogicalWindowSize.Height / 2;
                        LogicalOrigin = new Point(LogicalOrigin.X, newValueY);
                        Redraw();
                        break;
                        //MsgBox(se.Type.ToString())
                }
            }
        }

        #endregion

        #region "Routine per la Redraw()"

        public void Redraw()
        {
            Redraw(false);
        }

        public virtual void Redraw(bool forceGraphicCacheRebuild)
        {
            Graphics GR = null;
            try
            {
                // Se sono in fase di creazione del controllo o se il controllo e' gia' stato distrutto, posso uscire direttamente
                if ((this.IsDisposed))
                {
                    return;
                }

                // Se la PictureBox non e' visibile o se il suo layout e' sospeso, salto il ridisegno
                //If Not IsLoaded OrElse Not Visible Then
                if (!Visible || IsLayoutSuspended)
                {
                    //Exit Sub
                }


                //If Not IsLayoutSuspended Then
                UpdateDimensions();
                //End If

                // Check se il fattore di scala e' valido. 
                // Serve se chiamo la Redraw() quando la finestra e' minimizzata
                if ((ScaleFactor == 0.0) || (LogicalWidth == 0) || (LogicalHeight == 0))
                {
                    return;
                }

                // Check se ho un'area valida da visualizzare 
                //Debug.Assert(LogicalArea.IsNonZeroSized);


                // Questa routine va a disegnare sul buffer di secondo livello
                GR = GetScaledGraphicObject(myRedrawBackBuffer);
                if (GR == null)
                    return;

                // Pulisco con il colore di sfondo
                GR.Clear(BackgroundColor);

                if (myShowPictureBoxImage)
                {
                    DrawPictureBoxImage(GR);
                }

                // Disegno le griglie
                DrawGrids(GR);


                // Chiamo la Refresh()
                Refresh();

                // Aggiorno i dati delle scrollbar
                // NOTA: Devo farlo qui e non nella ShowLogicalWindow() perche' le routine che fanno lo zoom e il pan
                //       chiamano direttamente la Redraw() senza passare per la ShowLogicalWindow() 
                if (ShowScrollbars)
                {
                    UpdateScrollbars();
                }

                if (OnRedrawCompleted != null)
                {
                    OnRedrawCompleted(this, forceGraphicCacheRebuild);
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.ToUpper().Contains("CROSS-THREAD"))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
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
        public Image GetScreenShot()
        {
            try
            {
                System.Drawing.Size OutSize = new System.Drawing.Size(this.Width, this.Height);
                System.Drawing.Bitmap retValue = new System.Drawing.Bitmap(OutSize.Width, OutSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                Graphics gr = Graphics.FromImage(retValue);
                this.Redraw(true);
                gr.DrawImageUnscaled(myRedrawBackBuffer, 0, 0);
                return retValue;
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                return null;
            }
        }
        public bool SaveAScreenShot(string strDestFileName, ImageFormat _Format)
        {
            try
            {
                if (myRedrawBackBuffer != null)
                {
                    myRedrawBackBuffer.Save(strDestFileName, _Format);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Disegna l'immagine di sfondo della picturebox
        /// </summary>
        private void DrawPictureBoxImage(Graphics GR)
        {
            try
            {
                if (myPictureBoxImageGR == null)
                    return;
                switch (myPictureBoxImagePosition)
                {
                    case enBitmapOriginPosition.TopLeft:
                        myPictureBoxImageGR.Origin = Point.Empty;
                        break;
                    case enBitmapOriginPosition.Custom:
                        myPictureBoxImageGR.Origin = myPictureBoxImageCustomOrigin;
                        break;
                }
                myPictureBoxImageGR.Draw(GR);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }

        /// <summary>
        /// Disegna le griglie sul Graphics passatogli
        /// </summary>
        private void DrawGrids(Graphics GR)
        {
            // Disegno la griglia
            if (myShowGrid)
            {
                DrawGrid(GR, 0, GridStep, GridView, GridColor, SmartGridAdjust);
            }
            if (myShowGrid)
            {
                DrawAxes(GR);
            }
        }

        /// <summary>
        /// Disegna una griglia
        /// </summary>
        private void DrawGrid(Graphics GR, int GridInitialOffset, int GridStep, GridKind GridMode, Color GridColor, bool SmartAdjust)
        {
            try
            {
                // Check se il passo della griglia e' valido
                if (GridStep == 0)
                    return;

                // Check se il fattore di scala e' valido
                if ((ScaleFactor <= 0.0))
                {
                    return;
                }

                //Controllo la risoluzione della griglia per evitare sovraffollamenti ...
                if ((GridStep < myRulers.GetRulerStep()) && SmartAdjust)
                {
                    GridStep = (int)myRulers.GetRulerStep();
                }

                int InitialX = (int)(Math.Ceiling((double)(LogicalOrigin.X / GridStep)) * GridStep);
                int InitialY = (int)(Math.Ceiling((double)(LogicalOrigin.Y / GridStep)) * GridStep);
                Pen myPen = new Pen(GridColor);
                int FinalX = (int)(Math.Floor((double)((LogicalOrigin.X + LogicalWidth) / GridStep)) * GridStep);
                int FinalY = (int)(Math.Floor((double)((LogicalOrigin.Y + LogicalHeight) / GridStep)) * GridStep);
                int iIterX = 0;
                int iIterY = 0;
                switch (GridMode)
                {
                    case GridKind.Crosses:
                        for (iIterY = InitialY; iIterY <= FinalY; iIterY += GridStep)
                        {
                            for (iIterX = InitialX; iIterX <= FinalX; iIterX += GridStep)
                            {
                                GR.DrawLine(myPen, iIterX + GridInitialOffset - 10 / ScaleFactor, iIterY + GridInitialOffset, 
                                    iIterX + GridInitialOffset + 10 / ScaleFactor, iIterY + GridInitialOffset);
                                GR.DrawLine(myPen, iIterX + GridInitialOffset, iIterY + GridInitialOffset - 10 / ScaleFactor, 
                                    iIterX + GridInitialOffset, iIterY + GridInitialOffset + 10 / ScaleFactor);
                            }
                        }

                        break;
                    case GridKind.FullLines:
                        for (iIterY = InitialY; iIterY <= FinalY; iIterY += GridStep)
                        {
                            GR.DrawLine(myPen, LogicalOrigin.X, iIterY + GridInitialOffset, LogicalWidth + LogicalOrigin.X,
                                iIterY + GridInitialOffset);
                        }

                        for (iIterX = InitialX; iIterX <= FinalX; iIterX += GridStep)
                        {
                            GR.DrawLine(myPen, iIterX + GridInitialOffset, LogicalOrigin.Y, iIterX + GridInitialOffset,
                                LogicalHeight + LogicalOrigin.Y);
                        }

                        break;
                    case GridKind.Points:
                        for (iIterY = InitialY; iIterY <= FinalY; iIterY += GridStep)
                        {
                            for (iIterX = InitialX; iIterX <= FinalX; iIterX += GridStep)
                            {
                                GR.DrawLine(myPen, iIterX + GridInitialOffset - 1 / ScaleFactor, iIterY + GridInitialOffset,
                                    iIterX + GridInitialOffset + 1 / ScaleFactor, iIterY + GridInitialOffset);
                                GR.DrawLine(myPen, iIterX + GridInitialOffset, iIterY + GridInitialOffset - 1 / ScaleFactor, 
                                    iIterX + GridInitialOffset, iIterY + GridInitialOffset + 1 / ScaleFactor);
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }

        /// <summary>
        /// Disegna gli assi cartesiani
        /// </summary>
        private void DrawAxes(Graphics GR)
        {
            try
            {
                // Check se il fattore di scala e' valido
                if ((ScaleFactor <= 0.0))
                {
                    return;
                }

                GR.DrawLine(new Pen(AxesColor, -1), 0, LogicalOrigin.Y, 0, LogicalOrigin.Y + LogicalHeight);
                GR.DrawLine(new Pen(AxesColor, -1), LogicalOrigin.X, 0, LogicalOrigin.X + LogicalWidth, 0);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.ToString());
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }
        #endregion

        #region "Routine per la Refresh()"

        /// <summary>
        /// Copia myBackbufferBitmap sulla image della PictureBox (non tiene conto di cambiamenti di scala)
        /// Fa anche l'aggiornamento della posizione delle maniglie della finestra di selezione
        /// </summary>
        public void Refresh(bool _Invalidate = true)
        {
            Graphics GR = null;
            try
            {
                // Se sono in fase di creazione del controllo o se il controllo e' gia' stato distrutto, posso uscire direttamente
                if ((!this.Created) || (this.IsDisposed))
                {
                    return;
                }

                // Check se il fattore di scala e' valido. 
                // Serve se chiamo la Refresh() quando la finestra e' minimizzata
                if (ScaleFactor == 0.0)
                {
                    return;
                }

                // Oggetto graphics su cui andro' a tracciare
                GR = Graphics.FromImage(myRefreshBackBuffer);

                // Copio il buffer video di terzo livello su quello di secondo livello
                // NOTA: Inizialmente usavo la GR.DrawImageUnscaled(), ma e' inutile ricopiare la parte di bitmap che cade fuori
                //       dall'area client del controllo. Quindi uso la DrawImageUnscaledAndClipped()

                //LUCADN: ho rimosso l'impostazione a None di SmoothingMode
                //GR.SmoothingMode = Drawing2D.SmoothingMode.None

                GR.DrawImageUnscaledAndClipped(myRedrawBackBuffer, this.ClientRectangle);
                //MsgBox(String.Format("Redraw(): ClientRectangle = {0}", ClientRectangle))

                // Disegno i righelli
                if (myShowRulers)
                {
                    myRulers.Draw(GR);
                }

                // Adesso posso scalare il Graphics per disegnare i vari DesignObject
                ScaleGraphicObject(ref GR);

                if (_Invalidate)
                    Invalidate();

#if DEBUG_TIMING_REFRESH
			globalTimer.Trace("Refresh: tempo totale = ");
#endif
            }
            catch (Exception ex)
            {
                //MsgBox(ex.Message)
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
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

        #endregion

        #region "Routine per la OnPaint()"

        /// <summary>
        /// Disegna una preview di cio' che si vedra' in modalita' Stretch o Normal dopo aver terminato di ridimensionare la PictureBox.
        /// </summary>
        private void DrawResizePreview(Graphics GR)
        {
            // Pulisco lo sfondo
            GR.Clear(BackgroundColor);

            // Disegno le griglie
            ScaleGraphicObject(ref GR);
            DrawGrids(GR);
            GR.ResetTransform();

            // Check se la bitmap di partenza e' corretta, serve nel caso in cui Stilista parta minimizzato o con ClientArea nulla
            if ((myRedrawBackBuffer == null) || (myRedrawBackBuffer.Width == 0) || (myRedrawBackBuffer.Width == 0))
            {
                return;
            }

            // Se sono in modalita' Normal non ho bisogno di disegnare la bitmap scalata, mi basta:
            //  - fare una preview utilizzando l'ultima bitmap di refresh
            //  - aggiornare i righelli (sarebbe un "in piu'" ma lo faccio lo stesso, cosi' e' piu' completo)
            if ((ResizeMode == ResizeMode.Normal))
            {
                // Faccio la preview utilizzando l'ultima bitmap di redraw
                GR.DrawImage(myRedrawBackBuffer, Point.Empty);
                // Aggiorno i righelli
                if (myShowRulers)
                {
                    myRulers.Draw(GR);
                }
                return;
            }

            // NOTA: Se arrivo qui, vuol dire che sono in modalita' Stretch e che devo disegnare la bitmap scalata

            // Fattore di scala necessario per convertire la dimensione delle vecchia bitmap, creata dalla Redraw()
            // con un ClientArea diversa, in una bitmap adatta alla ClientArea attuale
            RECT actualPreviewArea = GraphicInfo.ToPhysicalRect(myLastVisibleAreaRequested);
            float tmpScaleFactor = Convert.ToSingle(Math.Max(actualPreviewArea.Width, 1)) / Convert.ToSingle(Math.Max(myResizeBeginEndPreviewArea.Width, 1));

            // Rettangolo di output su cui disegnare la bitmap [coordinate logiche]
            // Questo e' il rettangolo su cui verra' fatto lo shrink dell'ultima bitmap creata dalla Redraw()
            RECT bitmapOutputRect = new RECT();

            // Il mio unico punto fermo tra la vecchia bitmap (che avevo creato con una ClientArea completamente diversa) 
            // e la nuova ClientArea e' il punto di origine dell'ultima area visibile richiesta.
            // Quindi devo partire dalla "coordinata fisica attuale dell'ultima area visibile" e sottrarre la
            // "coordinata che aveva con la ClientArea iniziale", moltiplicata per il fattore di scala
            bitmapOutputRect.top = (int)(actualPreviewArea.top - (myResizeBeginEndPreviewArea.top * tmpScaleFactor));
            bitmapOutputRect.left = (int)(actualPreviewArea.left - (myResizeBeginEndPreviewArea.left * tmpScaleFactor));

            // Le dimensioni del rettangolo non richiedono spostamenti, basta adattarle con il fattore di scala
            bitmapOutputRect.Width = (int)(myRedrawBackBuffer.Width * tmpScaleFactor);
            bitmapOutputRect.Height = (int)(myRedrawBackBuffer.Height * tmpScaleFactor);

            // Disegno la bitmap 
            GR.DrawImage(myRedrawBackBuffer, bitmapOutputRect);

            // Aggiorno i righelli
            if (myShowRulers)
            {
                myRulers.Draw(GR);
            }
        }

        #endregion

        #region "Override della classe base"
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't allow the background to paint
            //MsgBox("OnPaintBackground")
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (IsLayoutSuspended || (!Visible) || (!Created) || (IsDisposed))
                {
                    return;
                }

                Graphics gr = e.Graphics;

                // Se sto ridimensionando, devo disegnare la preview e basta  
                if (myIsBetweenResizeBeginEnd)
                {
                    DrawResizePreview(gr);
                    return;
                }

                // Copio il backbuffer sullo schermo
                gr.DrawImage(myRefreshBackBuffer, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
                //MsgBox(String.Format("OnPaint(): ClipRectangle={0}", e.ClipRectangle))

                // Se sono in modalita' designer, non serve fare altro
                if (DesignMode)
                {
                    return;
                }

                // Posizione del mouse in vari tipi di coordinate
                Point physicalMousePos = this.PointToClient(MousePosition);
                Point logicalMousePos = GraphicInfo.ToLogicalPoint(physicalMousePos);

                // Adesso posso scalare il Graphics in modo definitivo
                ScaleGraphicObject(ref gr);

                // Flag che indica se si puo' disegnare il box di zoom/selezione oggetti
                bool drawSelectionBox = false;

                // Il tipo di azione da eseguire dipende dalla modalita' di click attuale
                switch (myClickAction)
                {
                    case enClickAction.None:
                        break; // TODO: might not be correct. Was : Exit Select
                    case enClickAction.MeasureDistance:
                        // NOTA: Le successive chiamate a funzioni di disegno richiedono un Graphics NON scalato
                        gr.ResetTransform();
                        // Disegno il righello usato per misurare
                        if (IsCtrlKeyPressed)
                        {
                            double ScaleFactor = MeasureSystem.MicronToCustomUnit(Convert.ToDouble(BackgroundImagePixelSize_Mic), myUnitOfMeasure, false);
                            myDistanceRuler.Painting(gr, ScaleFactor);
                        }
                        else
                        {
                            myDistanceRuler.Painting(gr);
                        }

                        break;
                    case enClickAction.Zoom:
                        // Disegno l'eventuale box di zoom/selezione oggetti
                        drawSelectionBox = IsDragging;
                        break; // TODO: might not be correct. Was : Exit Select
                }

                // NOTA: Le successive chiamate a funzioni di disegno richiedono un Graphics NON scalato
                gr.ResetTransform();

                // Disegno l'eventuale box di zoom/selezione oggetti
                if (drawSelectionBox && (!SelectionBox.IsInvalid))
                {
                    SelectionBox.Draw(gr);
                }


                if (FullPictureBoxCross)
                {
                }

                if (this.FullPictureBoxCross && ContainsMousePosition)
                {
                    FullCrossCursor.DrawCross(gr, logicalMousePos);
                }

                if (myShowMouseCoordinates)
                {
                    if (IsCtrlKeyPressed)
                    {
                        System.Drawing.Point BitmapCoord = default(System.Drawing.Point);
                        BitmapCoord.X = logicalMousePos.X / BackgroundImagePixelSize_Mic;
                        BitmapCoord.Y = logicalMousePos.Y / BackgroundImagePixelSize_Mic;
                        myCoordinatesBox.DrawCoordinateInfo(gr, BitmapCoord, true);
                    }
                    else
                    {
                        myCoordinatesBox.DrawCoordinateInfo(gr, logicalMousePos);
                    }
                }

            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
        }

        /// <summary>
        /// Evento chiamato quando comincio un ciclo di resize multipli.
        /// NOTA: Viene generato sia quando l'utente ridimensiona la finestra (sia con il mouse che da tastiera) che quando la sposta.
        /// </summary>
        private void OnResizeBegin(System.Object sender, System.EventArgs e)
        {
            // Notifico che sto gestendo una serie di eventi di resize
            myIsBetweenResizeBeginEnd = true;
            // Salvo la dimensione che la PictureBox ha all'inizio del ciclo di resize
            myBeginResizeClientArea = this.ClientRectangle;
            // Converto l'area di preview in coordinate fisiche della bitmap
            myResizeBeginEndPreviewArea = GraphicInfo.ToPhysicalRect(myLastVisibleAreaRequested);
        }

        /// <summary>
        /// Evento chiamato quando e' finto un ciclo di resize multipli.
        /// NOTA: Viene generato sia quando l'utente ridimensiona la finestra (sia con il mouse che da tastiera) che quando la sposta.
        /// </summary>
        private void OnResizeEnd(System.Object sender, System.EventArgs e)
        {
            // Notifico che ho finito di gestire una serie di eventi di resize
            myIsBetweenResizeBeginEnd = false;
            // Check se le dimensioni della PictureBox sono effettivamente cambiate
            // NOTA: Serve perche' questa routine viene chiamata durante la gestione dell''evento OnResizeEnd(),
            //       che viene chiamato anche quando sposto la form contenente la PictureBox.
            if ((myBeginResizeClientArea != this.ClientRectangle))
            {
                // Genero un evento di cambio dimensione, in modo da forzare il ridisegno
                OnSizeChanged(e);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                // MsgBox("OnSizeChanged()")
                // NOTA: Quando cambio il valore di Me.AutoScroll, viene generato un evento Resize (e/o SizeChanged)
                //       anche se la dimensione della PictureBox non viene effettivamente modificata
                if (myIsChangingAutoScroll)
                    return;

                // Aggiorno i valori interni
                // NOTA BENE: Questo mi va a modificare l'area logica che sto visualizzando!
                //            E' per questo che mi serve la variabile myLastVisibleAreaRequested, perche' altrimenti
                //            ad ogni Resize() perderei completamente l'area che stavo visualizzando
                GraphicInfo.PhysicalWidth = this.Width;
                GraphicInfo.PhysicalHeight = this.Height;

                // Se la finestra e' minimizzata esce subito
                if ((Width < 1) || (Height < 1))
                    return;
                //MsgBox("Resize to (" + picPictureBox.Width.ToString() + "," + picPictureBox.Height.ToString() + ")")

                if (!IsLoaded & ResizeMode != ResizeMode.Stretch)
                    return;

                // Check se l'utente sta ridimensionando la finestra (e quindi mi stanno arrivando tutta una serie di Resize() di fila)
                if (myIsBetweenResizeBeginEnd)
                {
                    // Se sono in modalita' Stretch, imposto l'area logica in modo che sia uguale a quella 
                    // che andrei a visualizzare se il resize terminasse con queste dimensioni.
                    // Se non lo facessi, la bitmap di preview verrebbe disegnata in una posizione completamente sbagliata
                    // NOTA: Questo e' uno dei pochi casi in cui la LogicalArea va impostata direttamente.
                    // NOTA: Non va fatto in modalita' Normal, altrimenti righelli e icone vengono disegnati nel posto sbagliato
                    if ((ResizeMode == ResizeMode.Stretch))
                    {
                        LogicalArea = VisibleAreaToLogicalArea(myLastVisibleAreaRequested);
                    }

                    // Devo invalidare "a mano" la finestra, altrimenti non mi lascia disegnare sopra la parte di PictureBox che era gia' visibile 
                    Invalidate();
                    return;
                }

                // Se necessario, aggiorno le bitmap del buffer video
                // NOTA: La UpdateDimensions() ritorna true se le bitmap sono state aggiornate, false altrimenti.
                UpdateDimensions();

                // Check se la area logica deve variare in modo da mantenere l'ultima visualizzazione 
                if ((ResizeMode == ResizeMode.Stretch))
                {
                    // Faccio uno zoom all'ultima area visibile richiesta, senza salvarlo nella history.
                    // NOTA: L'area visibile richiesta verrebbe sovrascritta durante l'esecuzione della ZoomToLogicalWindow(),
                    //       ma siccome io passo lo stesso valore che aveva prima, in pratica e' come se fosse invariante.
                    ShowLogicalWindow(myLastVisibleAreaRequested, false);
                }
                else
                {
                    // Devo sempre fare una Redraw(), perche' se faccio una Refresh() posso avere errori nel ridisegno nel caso in cui:
                    //  - diminuisco la dimensione della finestra 
                    //  - cambio la View attuale 
                    //  - reingrandisco la finestra, con dimensioni che non obbligano a ricreare le bitmap
                    Redraw();
                }

            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
            }
            finally
            {
                // Devo chiamare la routine della classe base, altrimenti gli eventi OnSizeChanged() e OnResize() non vengono mai generati
                base.OnSizeChanged(e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            //MsgBox("OnMouseEnter()")
            // Aggiorno il cursore al valore di default
            CurrentCursor = DefaultCursor;
            // Chiamo la corrispondente funzione della classe base
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            try
            {
                //MsgBox("OnMouseLeave()")
                // Se avevo il cursore a croce a pieno schermo, devo cancellarlo, altrimenti mi ritrovo 
                // una riga che attraversa tutta la pictureBox e rimane li' fino a qunado non rientro con il mouse
                if (this.FullPictureBoxCross)
                {
                    // NOTA: Non serve chiamare la myCrossCursor.ResetCrossPosition(), tanto la posizione da disegnare
                    //       viene passata al cursore direttamente nella OnPaint()
                    Invalidate();
                }
                // Chiamo la corrispondente funzione della classe base
                base.OnMouseLeave(e);
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                //MsgBox("OnMouseWheel()")
                // NOTA: Non utilizzare la variabile MousePosition, perche' ritorna la posizione 
                //       del mouse in coordinate DELLO SCHERMO (rispetto all'angolo superiore sinistro)
                Point MouseLogicalPosition = default(Point);
                MouseLogicalPosition.X = (int)(e.X / ScaleFactor + LogicalOrigin.X);
                MouseLogicalPosition.Y = (int)(e.Y / ScaleFactor + LogicalOrigin.Y);

                // NOTA: Con la rotellina del mouse mi sposto come con il PAN con shift premuto,
                //       cioe' il pan con spostamento MINORE
                if (e.Delta > 0)
                {
                    if (IsCtrlKeyPressed)
                    {
                        PanLeft(PanFactorWithShift);
                        return;
                    }
                    if (IsShiftKeyPressed)
                    {
                        PanUp(PanFactorWithShift);
                        return;
                    }
                    // Fa uno Zoom In rispetto alla posizione del mouse
                    ZoomForward(ref MouseLogicalPosition);
                }
                else
                {
                    if (IsCtrlKeyPressed)
                    {
                        PanRight(PanFactorWithShift);
                        return;
                    }
                    if (IsShiftKeyPressed)
                    {
                        PanDown(PanFactorWithShift);
                        return;
                    }

                    // Fa uno Zoom Out rispetto alla posizione del mouse
                    ZoomBack(ref MouseLogicalPosition);
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        /// <summary>
        /// Override dei parametri di creazione del controllo
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                // Imposto il bordo del controllo
                const int WS_BORDER = 0x800000;
                const int WS_EX_STATICEDGE = 0x20000;
                CreateParams cp = base.CreateParams;
                switch (myBorderStyle)
                {
                    case BorderStyle.FixedSingle:
                        cp.Style = cp.Style | WS_BORDER;
                        break;
                    case BorderStyle.Fixed3D:
                        cp.ExStyle = cp.ExStyle | WS_EX_STATICEDGE;
                        break;
                }

                return cp;
            }
        }

        private void AddResizeHandlers()
        {
            Form parentForm = FindForm();
            // Check se il controllo e' nullo
            if ((parentForm == null))
            {
                return;
            }
            if ((parentForm.IsMdiChild))
            {
                parentForm = parentForm.MdiParent;
                if ((parentForm == null))
                {
                    return;
                }
            }
            parentForm.ResizeBegin += this.OnResizeBegin;
            parentForm.ResizeEnd += this.OnResizeEnd;
        }

        private void RemoveResizeHandlers()
        {
            Form parentForm = FindForm();
            // Check se il controllo e' nullo
            if ((parentForm == null))
            {
                return;
            }
            parentForm.ResizeBegin -= this.OnResizeBegin;
            parentForm.ResizeEnd -= this.OnResizeEnd;
        }

        #endregion

        #region "Gestione dello Zoom"

        #region "Varie"

        /// <summary>
        /// Ritorna true se sono al minimo livello di Zoom Out ammissibile
        /// </summary>
        [Browsable(false)]
        public bool MinimumZoomReached
        {
            get
            {
                // Ritorno true se IL PROSSIMO zoom mi porterebbe fuori dai limiti
                int nextHeight = (int)(LogicalHeight * ZoomMultiplier);
                int nextWidth = (int)(LogicalWidth * ZoomMultiplier);
                return nextHeight > MaxLogicalWindowSize.Height || nextWidth > MaxLogicalWindowSize.Width;
            }
        }

        /// <summary>
        /// Ritorna true se sono al massimo livello di Zoom In ammissibile
        /// </summary>
        [Browsable(false)]
        public bool MaximumZoomReached
        {
            get
            {
                // Ritorno true se IL PROSSIMO zoom mi porterebbe fuori dai limiti
                int nextHeight = (int)(LogicalHeight / ZoomMultiplier);
                int nextWidth = (int)(LogicalWidth / ZoomMultiplier);
                return nextHeight < myMinLogicalWindowSize.Height || nextWidth < myMinLogicalWindowSize.Width;
            }
        }

        #endregion

        #region "Zoom in"

        /// <summary>
        /// Effettua uno "Zoom In" mettendo al centro dello schermo il punto passatogli
        /// </summary>
        public void ZoomForwardUsingCenter(Point ZoomCenter)
        {
            try
            {
                // Se ho raggiunto lo zoom massimo, esco direttamente
                if (MaximumZoomReached)
                {
                    if (OnMaximumZoomLevelReached != null)
                    {
                        OnMaximumZoomLevelReached(this);
                    }
                    return;
                }

                // Faccio lo zoom dell'area logica
                // NOTA: Quando faccio uno zoom in, l'area logica visibile viene ridotta
                RECT tmpArea = LogicalArea.ExpandFromFixedPoint(1f / ZoomMultiplier, LogicalCenter);
                // Riporto il centro della nuova area logica sul punto voluto
                tmpArea.Offset(ZoomCenter.X - LogicalCenter.X, ZoomCenter.Y - LogicalCenter.Y);

                // Devo espandere e spostare della stessa quantita' anche l'ultima area visualizzata 
                myLastVisibleAreaRequested = myLastVisibleAreaRequested.ExpandFromFixedPoint(1f / ZoomMultiplier, LogicalCenter);
                myLastVisibleAreaRequested.Offset(ZoomCenter.X - LogicalCenter.X, ZoomCenter.Y - LogicalCenter.Y);

                // Aggiorno l'area logica visualizzata
                LogicalArea = tmpArea;

                // Ridisegno la finestra
                Redraw();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        /// <summary>
        /// Effettua uno "Zoom In" rispetto al centro logico della finestra
        /// </summary>
        public void ZoomForwardOnLogicalCenter()
        {
            ZoomForwardUsingCenter(LogicalCenter);
        }

        /// <summary>
        /// Effettua uno "Zoom In" mantenendo "fisso" il punto che gli viene passato
        /// </summary>
        public void ZoomForward(ref Point LogicalPosition)
        {
            // Se ho raggiunto lo zoom massimo, esco direttamente
            if (MaximumZoomReached)
            {
                if (OnMaximumZoomLevelReached != null)
                {
                    OnMaximumZoomLevelReached(this);
                }
                return;
            }

            // Trovo la distanza del punto passatomi dal centro dello schermo
            Point distance = new Point(LogicalPosition.X - LogicalCenter.X, LogicalPosition.Y - LogicalCenter.Y);

            // Trovo la distanza dal centro dello schermo che avrei con la prossima visualizzazione e calcolo 
            // il centro dello schermo della prossima visualizzazione (rispetto al punto passatomi)
            distance.X = (int)(distance.X / ZoomMultiplier);
            distance.Y = (int)(distance.Y / ZoomMultiplier);
            Point newZoomCenter = new Point(LogicalPosition.X - distance.X, LogicalPosition.Y - distance.Y);

            // Sposto lo zoom sul punto calcolato
            ZoomForwardUsingCenter(newZoomCenter);
        }

        #endregion

        #region "Zoom out"

        /// <summary>
        /// Effettua uno "Zoom Out" mettendo al centro dello schermo il punto passatogli
        /// </summary>
        public void ZoomBackUsingCenter(Point ZoomCenter)
        {
            try
            {
                // Se ho raggiunto lo zoom minimo, esco direttamente
                if (MinimumZoomReached)
                {
                    if (OnMinimumZoomLevelReached != null)
                    {
                        OnMinimumZoomLevelReached(this);
                    }
                    return;
                }

                // Faccio lo zoom dell'area logica
                // NOTA: Quando faccio uno zoom out, l'area logica visibile viene espansa
                RECT tmpArea = LogicalArea.ExpandFromFixedPoint(ZoomMultiplier, LogicalCenter);
                // Riporto il centro della nuova area logica sul punto voluto
                tmpArea.Offset(ZoomCenter.X - LogicalCenter.X , ZoomCenter.Y - LogicalCenter.Y);

                // Devo espandere e spostare della stessa quantita' anche l'ultima area visualizzata 
                myLastVisibleAreaRequested = myLastVisibleAreaRequested.ExpandFromFixedPoint(ZoomMultiplier, LogicalCenter);
                myLastVisibleAreaRequested.Offset(ZoomCenter.X - LogicalCenter.X, ZoomCenter.Y - LogicalCenter.Y);


                // Aggiorno l'area logica visualizzata
                LogicalArea = tmpArea;

                // Ridisegno la finestra
                Redraw();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        /// <summary>
        /// Effettua uno "Zoom Out" rispetto al centro logico della finestra
        /// </summary>
        public void ZoomBackOnLogicalCenter()
        {
            ZoomBackUsingCenter(LogicalCenter);
        }

        /// <summary>
        /// Effettua uno "Zoom Out" mantenendo "fisso" il punto che gli viene passato
        /// </summary>

        public void ZoomBack(ref Point LogicalPosition)
        {
            // Se ho raggiunto lo zoom minimo, esco direttamente
            if (MinimumZoomReached)
            {
                if (OnMinimumZoomLevelReached != null)
                {
                    OnMinimumZoomLevelReached(this);
                }
                return;
            }

            // Trovo la distanza del punto passatomi dal centro dello schermo
            Point distance = new Point(LogicalPosition.X - LogicalCenter.X, LogicalPosition.Y - LogicalCenter.Y);

            // Trovo la distanza dal centro dello schermo che avrei con la prossima visualizzazione e calcolo 
            // il centro dello schermo della prossima visualizzazione (rispetto al punto passatomi)
            distance.X = (int)(distance.X * ZoomMultiplier);
            distance.Y = (int)(distance.Y * ZoomMultiplier);
            Point newZoomCenter = new Point(LogicalPosition.X - distance.X, LogicalPosition.Y - distance.Y);

            // Sposto lo zoom sul punto calcolato
            ZoomBackUsingCenter(newZoomCenter);
        }

        #endregion

        #region "Zoom"

        public void ZoomToDefaultRect()
        {
            try
            {
                if (Image != null)
                {
                    ZoomToFit();
                }
                else
                {
                    // Zoom al rettangolo di default (senza centrarlo nella finestra)
                    ShowLogicalWindow(DefaultRect, false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }
        public void ZoomToFit()
        {
            try
            {
                if (Image != null)
                {
                    RECT ImgR = new RECT(ImageCustomOrigin.X, ImageCustomOrigin.Y, ImageCustomOrigin.X + Image.Width, ImageCustomOrigin.Y + Image.Height);
                    RECT PhysicalR = default(RECT);
                    PhysicalR.left = ImgR.left * BackgroundImagePixelSize_Mic;
                    PhysicalR.top = ImgR.top * BackgroundImagePixelSize_Mic;
                    PhysicalR.Width = ImgR.Width * BackgroundImagePixelSize_Mic;
                    PhysicalR.Height = ImgR.Height * BackgroundImagePixelSize_Mic;
                    ShowLogicalWindow(PhysicalR, true);
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        #endregion

        [Browsable(false)]
        public static RECT DefaultRect
        {
            get
            {
                // Un rettangolo di default per me vale circa 10 x 10 cm, tengo lo zero in alto a sx
                // e faccio in modo che l'incrocio degli assi sia visibile (tengo circa 3mm in piu') 
                RECT _Rect = default(RECT);
                _Rect.left = -3000;
                // -3mm 
                _Rect.top = -3000;
                // -3mm 
                _Rect.right = 100000;
                // 10cm 
                _Rect.bottom = 100000;
                // 10cm 
                return _Rect;
            }
        }

        #endregion

        #region "Gestione del Pan"
        public virtual void PanHome()
        {
        }
        public virtual void PanEnd()
        {
        }
        public virtual void PageUp()
        {
        }
        public virtual void PageDown()
        {
        }

        /// <summary>
        /// Effettua uno spostamento in basso di un valore pari alla "percentuale della finestra logica" passatagli
        /// </summary>
        public virtual void PanDown(float Percent)
        {
            try
            {
                // Check se ho raggiunto il limite visualizzabile
                if (LogicalOrigin.Y + LogicalHeight > MaxLogicalWindowSize.Height / 2)
                    return;
                // Calcolo l'offset di cui spostarmi 
                int offsetY = Convert.ToInt32(Convert.ToSingle(LogicalHeight) * (Percent / 100f));
                // Sposto l'origine
                LogicalOrigin = new Point(LogicalOrigin.X, LogicalOrigin.Y + offsetY);
                // Devo spostare della stessa quantita' anche l'ultima area visualizzata 
                myLastVisibleAreaRequested.Offset(0, offsetY);
                // Faccio un ridisegno
                Redraw();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        /// <summary>
        /// Effettua uno spostamento in alto di un valore pari alla "percentuale della finestra logica" passatagli
        /// </summary>
        public virtual void PanUp(float Percent)
        {
            // Check se ho raggiunto il limite visualizzabile
            if (LogicalOrigin.Y < -MaxLogicalWindowSize.Height / 2)
                return;
            // Calcolo l'offset di cui spostarmi 
            int offsetY = Convert.ToInt32(Convert.ToSingle(LogicalHeight) * (Percent / 100f));
            // Sposto l'origine
            LogicalOrigin = new Point(LogicalOrigin.X, LogicalOrigin.Y - offsetY);
            // Devo spostare della stessa quantita' anche l'ultima area visualizzata 
            myLastVisibleAreaRequested.Offset(0, -offsetY);
            // Faccio un ridisegno
            Redraw();
        }

        /// <summary>
        /// Effettua uno spostamento a sinistra di un valore pari alla "percentuale della finestra logica" passatagli
        /// </summary>
        public virtual void PanLeft(float Percent)
        {
            // Check se ho raggiunto il limite visualizzabile
            if (LogicalOrigin.X < -MaxLogicalWindowSize.Width / 2)
                return;
            // Calcolo l'offset di cui spostarmi
            int offsetX = Convert.ToInt32(Convert.ToSingle(LogicalWidth) * (Percent / 100f));
            // Sposto l'origine
            LogicalOrigin = new Point(LogicalOrigin.X - offsetX, LogicalOrigin.Y);
            // Devo spostare della stessa quantita' anche l'ultima area visualizzata 
            myLastVisibleAreaRequested.Offset(-offsetX, 0);
            // Faccio un ridisegno
            Redraw();
        }

        /// <summary>
        /// Effettua uno spostamento a destra di un valore pari alla "percentuale della finestra logica" passatagli
        /// </summary>
        public virtual void PanRight(float Percent)
        {
            // Check se ho raggiunto il limite visualizzabile
            if (LogicalOrigin.X + LogicalWidth > MaxLogicalWindowSize.Width / 2)
                return;
            // Calcolo l'offset di cui spostarmi
            int offsetX = Convert.ToInt32(Convert.ToSingle(LogicalWidth) * (Percent / 100f));
            // Sposto l'origine
            LogicalOrigin = new Point(LogicalOrigin.X + offsetX, LogicalOrigin.Y);
            // Devo spostare della stessa quantita' anche l'ultima area visualizzata 
            myLastVisibleAreaRequested.Offset(offsetX, 0);
            // Faccio un ridisegno
            Redraw();
        }

        #endregion

        #region "Gestione input da tastiera"

        /// <summary>
        /// Flag che indica se la ProcessCmdKey sta processando un carattere che non arriva da tastiera
        /// </summary>

        private bool myAlreadyInProcessCmdKey = false;
        /// <summary>
        /// Funzione per il processamento dei messaggi da tastiera che arrivano da altri controlli
        /// </summary>
        public bool ProcessKeyboardKey(ref Message msg, Keys keyData)
        {
            return ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Override della funzione che recupera i messaggi da windows
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Check se sto tentando una ricorsione infinita
            if (myAlreadyInProcessCmdKey)
            {
                return false;
            }

            try
            {
                myAlreadyInProcessCmdKey = true;

                // Se e' attivo il tracciamento punto-punto, ha la precedenza sul processamento dei comandi da tastiera
                // NOTA: La ProcessKeyboardKey() ritorna true se il messaggio e' stato gestito

                // Flag che indica se e' premuto SHIFT
                bool shiftIsPressed = (keyData & Keys.Shift) == Keys.Shift;
                bool ctrlIsPressed = (keyData & Keys.Control) == Keys.Control;

                Keys msgKey = (Keys)msg.WParam.ToInt32();

                // Output di debug

                if ((msgKey != Keys.Control) && (msgKey != Keys.Shift) && (msgKey != Keys.Alt) && (msgKey != Keys.ControlKey) && (msgKey != Keys.ShiftKey))
                {
                    //??
                }

                switch (msgKey)
                {
                    case Keys.Left:
                        // -> Pan Left
                        if (shiftIsPressed)
                        {
                            PanLeft(PanFactorWithShift);
                        }
                        else
                        {
                            PanLeft(PanFactorNoShift);
                        }
                        return true;
                    case Keys.Right:
                        // -> Pan Right
                        if (shiftIsPressed)
                        {
                            PanRight(PanFactorWithShift);
                        }
                        else
                        {
                            PanRight(PanFactorNoShift);
                        }
                        return true;
                    case Keys.PageDown:
                        PageDown();
                        break;
                    case Keys.PageUp:
                        PageUp();
                        break;
                    case Keys.Up:
                        // -> Pan Up
                        if (shiftIsPressed)
                        {
                            PanUp(PanFactorWithShift);
                        }
                        else
                        {
                            PanUp(PanFactorNoShift);
                        }
                        return true;
                    case Keys.Down:
                        // -> Pan Down
                        if (shiftIsPressed)
                        {
                            PanDown(PanFactorWithShift);
                        }
                        else
                        {
                            PanDown(PanFactorNoShift);
                        }
                        return true;
                    case Keys.End:
                        PanEnd();
                        break;
                    case Keys.Home:
                        PanHome();
                        break;
                    case Keys.Add:
                        // -> Zoom In
                        ZoomForwardOnLogicalCenter();
                        return true;
                    case Keys.Subtract:
                        // -> Zoom Out
                        ZoomBackOnLogicalCenter();
                        return true;
                    case Keys.Escape:
                        // -> Annullamento azione (o deselezione oggetti)
                        // Se ho il tasto sinistro del mouse premuto, annullo la trasformazione che stavo facendo
                        // Se non ho pulsanti premuti, deseleziono eventuali funzioni selezionate
                        switch (MouseButtons)
                        {
                            case MouseButtons.Left:
                                // Cancello gli eventuali dati temporanei usati tra un MouseDown e un MouseUp e ridisegno la finestra
                                ResetTemporaryData();
                                Invalidate();
                                break;
                            case MouseButtons.None:
                                break;
                                // Deseleziono le funzioni selezionate 
                                // NOTA: La finestra viene ridisegnata automaticamente

                        }
                        return true;
                }

                // Chiamo la routine della classe base
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
                return false;
            }
            finally
            {
                myAlreadyInProcessCmdKey = false;
            }
        }


        #endregion

        #region "Routine per calcolare/mostrare finestre logiche"

        /// <summary>
        /// show the window logic is required.
        /// if saveinzoomhistory is true, save the new zoom in the history of the zoom lens.
        /// if centerwindow is true, the window is centered in the picturebox, otherwise it is left aligned to the left.
        /// if addemptyborder's true zoom decreases slightly, in order to have an empty frame around the area desired.
        /// if excluderulersarea is true (and if the rulers are visible through the window mappata) is in the area of picturebox.
        /// not covered by rulers
        /// </summary>
        public void ShowLogicalWindow(RECT LogicalWindow, bool CenterWindow = true, bool AddEmptyBorder = true, bool ExcludeRulersArea = true)
        {
            try
            {
                // Check se il rettangolo passato e' valido. Se non lo e' faccio lo zoom al rettangolo di default.
                // NOTA: Se ho una serie di funzioni macchina selezionate, otterro' un ingombro NON VALIDO,
                //       quindi faro' lo zoom al rettangolo di default
                if (LogicalWindow.IsZeroSized)
                {
                    ZoomToDefaultRect();
                    return;
                }

                // Imposto l'area visualizzabile 
                LogicalArea = VisibleAreaToLogicalArea(LogicalWindow, CenterWindow, AddEmptyBorder, ExcludeRulersArea);

                // Ridisegno la finestra
                Redraw();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        /// <summary>
        /// Ritorna l'area logica a cui e' necessario effettuare lo zoom per garantire la visibilita' dell'area desiderata.
        /// Le due aree non coincidono in quanto l'area ritornata rispetta la proporzione tra altezza e larghezza
        /// e se necessario, tiene conto dell'area occupata dai righelli e della centratura dell'area desiderata.
        /// Se CenterWindow e' true, l'area desiderata viene centrata nella picturebox, altrimenti viene lasciata allineata a sinistra
        /// Se AddEmptyBorder e' true lo zoom diminuisce lievemente, in modo da avere una cornice vuota attorno all'area desiderata
        /// Se ExcludeRulersArea e' true (e se i righelli sono visibili) l'area desiderata
        /// viene mappata nella parte della picturebox non coperta dai righelli
        /// </summary>
        private RECT VisibleAreaToLogicalArea(RECT visibleArea, bool CenterWindow = true, bool AddEmptyBorder = true, bool ExcludeRulersArea = true)
        {
            // Check di sicurezza
            if (visibleArea.IsZeroSized)
            {
                return new RECT();
            }

            // Mi assicuro che il rettangolo sia normalizzato
            visibleArea.NormalizeRect();

            // Aggiorno l'ultima area visibile che mi e' stata richiesta
            myLastVisibleAreaRequested = visibleArea;

            // Dimensioni orizzontali e verticali del controllo.
            // NOTA: Uso il ClientRectangle.Width al posto di Width perche' cosi' tengo conto delle eventuali scrollbar.
            float clientWidth = this.ClientRectangle.Width;
            float clientHeight = this.ClientRectangle.Height;

            // Spazio occupato dai righelli, e' diverso da zero solo se devo disegnare i righelli 
            // e se e' attiva l'opzione ExcludeRulersArea
            float rulersPhysicalSize = 0;

            // Calcolo lo spazio occupato dai righelli
            // Serve per impedire che la parte superiore sinistra dell'area richiesta venga coperta dai righelli
            if (myShowRulers && ExcludeRulersArea)
            {
                rulersPhysicalSize = myRulers.Size;
            }

            // Se e' stata richiesta una cornice vuota devo aumentare lievemente 
            // lo spazio richiesto dalla finestra logica sulla coordinata in uso.
            // Modifico entrambe le coordinate, tanto usero' solo quella che mi serve
            if (AddEmptyBorder)
            {
                int widthBorder = Convert.ToInt32(visibleArea.Width / 18);
                int heightBorder = Convert.ToInt32(visibleArea.Height / 18);
                visibleArea.top -= heightBorder;
                visibleArea.bottom += heightBorder;
                visibleArea.left -= widthBorder;
                visibleArea.right += widthBorder;
                //Debug.Assert(visibleArea.top == myLastVisibleAreaRequested.top - heightBorder);
                //Debug.Assert(visibleArea.bottom == myLastVisibleAreaRequested.bottom + heightBorder);
                //Debug.Assert(visibleArea.left == myLastVisibleAreaRequested.left - widthBorder);
                //Debug.Assert(visibleArea.right == myLastVisibleAreaRequested.right + widthBorder);
            }

            // Spazio disponibile per tracciare.
            // Se la client area diventa talmente piccola che ci stanno solo i righelli,
            // faccio comunque in modo da tenermi un pixel per tracciare
            float availableWidth = Math.Max(clientWidth - rulersPhysicalSize, 1);
            float availableHeight = Math.Max(clientHeight - rulersPhysicalSize, 1);

            // Fattori di scala corrispondenti alla piu' piccola e alla piu' grande finestra visualizzabile
            float minScaleFactor = Math.Min(availableWidth / MinLogicalWindowSize.Width, availableHeight / MinLogicalWindowSize.Height);
            float maxScaleFactor = Math.Min(availableWidth / MaxLogicalWindowSize.Width, availableHeight / MaxLogicalWindowSize.Height);
            if (availableWidth == 1)
            {
                availableWidth = 1;
            }
            // Trovo i due fattori di scala che mi portano ad avere la finestra desiderata 
            // a piena dimensione verticale o orizzontale
            float horzScaleFactor = availableWidth / visibleArea.Width;
            float vertScaleFactor = availableHeight / visibleArea.Height;

            // Check se i fattori di scala sono validi
            // NOTA: Possono diventare nulli quando rimpicciolisco la finestra fino ad arrivare
            //       ad una dimensione pari o minore di quella dei righelli
            // TODO: Questo e' solo un workaround, in questo caso la visualizzazione diventa sbagliata
            if ((horzScaleFactor <= 0))
                horzScaleFactor = maxScaleFactor;
            if ((vertScaleFactor <= 0))
                vertScaleFactor = maxScaleFactor;

            // Nuovo fattore di scala da usare
            // Dei due fattori di scala devo prendere quello piu' piccolo, in modo che 
            // visibleArea.Width e visibleArea.Height staranno dentro all'area finale.
            // Questo sara' il fattore di scala che la PictureBox avrebbe se visualizzasse la visibleArea finale.
            float newScaleFactor = Math.Min(horzScaleFactor, vertScaleFactor);
            //Debug.Assert(newScaleFactor != 0, "Trovato fattore di scala non valido");

            // Check se il fattore di scala mi porterebbe ad una finestra troppo grande o troppo piccola per essere visualizzata
            if ((newScaleFactor > minScaleFactor))
                newScaleFactor = minScaleFactor;
            if ((newScaleFactor < maxScaleFactor))
                newScaleFactor = maxScaleFactor;

            // Dimensioni dell'area logica che la PictureBox visualizzerebbe con il nuovo fattore di scala
            // NOTA: Queste dimensioni comprendono l'ingombro degli eventuali righelli
            float newLogicalHeight = clientHeight / newScaleFactor;
            float newLogicalWidth = clientWidth / newScaleFactor;

            // Dimensione logica che i righelli avrebbero con il nuovo fattore di scala
            int rulersLogicalSize = (int)(rulersPhysicalSize / newScaleFactor);

            // Offset orizzontale e verticale da sommare all'area visibile
            float horizontalOffset = 0;
            float verticalOffset = 0;

            // Se richesto, calcolo gli offset necessari per il centraggio
            // NOTA: Faccio entrambi i centraggi, tanto nel caso standard uno dei due offset rimane a zero.
            //       Invece mi servono entrambi i centraggi nel caso in cui tento uno zoom ad un oggetto piccolissimo, cioe' quando supero minScaleFactor
            if (CenterWindow)
            {
                // NOTA: Questi offset NON comprendono l'ingombro degli eventuali righelli
                verticalOffset = Math.Abs((newLogicalHeight - rulersLogicalSize - visibleArea.Height) / 2);
                horizontalOffset = Math.Abs((newLogicalWidth - rulersLogicalSize - visibleArea.Width) / 2);
            }

            // Aggiorno la posizione dell'area da visualizzare 
            RECT logicalAreaToShow = new RECT();
            logicalAreaToShow.left = (int)(visibleArea.left - rulersLogicalSize - horizontalOffset);
            logicalAreaToShow.top = (int)(visibleArea.top - rulersLogicalSize - verticalOffset);

            // Aggiorno entrambe le dimensioni dell'area da visualizzare in modo da riflettere il nuovo fattore di scala
            // NOTA: Le dimensioni dell'area logica che visualizzo comprendono i righelli,
            //       quindi non devo sottrarre l'ingombro dei righelli
            // NOTA: Qui va usata Width al posto di ClientRectangle.Width perche' altrimenti il centraggio
            //       non avviene correttamente quando le scrollbar sono visualizzate
            logicalAreaToShow.Width = (int)(this.Width / newScaleFactor);
            logicalAreaToShow.Height = (int)(this.Height / newScaleFactor);
            logicalAreaToShow.NormalizeRect();

            // Ritorno l'area logica da visualizzare 
            return logicalAreaToShow;
        }

        #endregion

        #region "Preview delle trasformazioni"

        /// <summary>
        /// Ritorna i fattori di scala da utilizzare in X e in Y per la preview della trasformazione
        /// </summary>
        public System.Drawing.PointF CalculateScaleFactors(Point ScalingCenter, Point FirstScalePoint, Point SecondScalePoint, bool mantainAspectRatio = false)
        {
            try
            {
                // Distanza in X e Y
                double DeltaX = SecondScalePoint.X - FirstScalePoint.X;
                double DeltaY = SecondScalePoint.Y - FirstScalePoint.Y;

                // Facio gli eventuali aggiustamenti per mantenere il rapporto tra X e Y
                if (mantainAspectRatio)
                {
                    if (Math.Abs(DeltaX) > Math.Abs(DeltaY))
                    {
                        DeltaY = DeltaX * ((FirstScalePoint.Y - ScalingCenter.Y) / (FirstScalePoint.X - ScalingCenter.X));
                    }
                    else
                    {
                        DeltaX = DeltaY * ((FirstScalePoint.X - ScalingCenter.X) / (FirstScalePoint.Y - ScalingCenter.Y));
                    }
                }

                // Valori comuni per la riscalatura in X e in Y, il valore di riscalatura in X e':
                //    DeltaX * ((actualPoint.X - ScalingCenter.X) / (FirstScalePoint.X - ScalingCenter.X))
                // Siccome actualPoint.X varia da punto a punto, la parte comune a tutti i punti e': 
                //    DeltaX * 1.0 / (FirstScalePoint.X - ScalingCenter.X)
                // che viene conglobata nella CommonFactorX
                // Un analogo discorso vale per la CommonFactorY
                double commonFactorX = 0.0;
                if (FirstScalePoint.X != ScalingCenter.X)
                {
                    commonFactorX = DeltaX / (FirstScalePoint.X - ScalingCenter.X);
                }
                double commonFactorY = 0.0;
                if (FirstScalePoint.Y != ScalingCenter.Y)
                {
                    commonFactorY = DeltaY / (FirstScalePoint.Y - ScalingCenter.Y);
                }

                // La scrittura originale era:
                //    ScaledXRatio = (FirstScalePoint.X + DeltaX - ScalingCenter.X) / (FirstScalePoint.X - ScalingCenter.X)
                // Da cui si ottiene:
                //    ScaledXRatio = (FirstScalePoint.X - ScalingCenter.X + DeltaX) / (FirstScalePoint.X - ScalingCenter.X)
                //                 = (FirstScalePoint.X - ScalingCenter.X) / (FirstScalePoint.X - ScalingCenter.X) + DeltaX / (FirstScalePoint.X - ScalingCenter.X)
                //                 = 1.0 + DeltaX / (FirstScalePoint.X - ScalingCenter.X)
                //                 = 1.0 + commonFactorX 
                // Un analogo discorso vale per la preview.ScaledYRatio
                return new System.Drawing.PointF((float)(1.0f + commonFactorX), (float)(1.0f + commonFactorY));
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
                return new System.Drawing.PointF(1.0f, 1.0f);
            }
        }
        #endregion

        #region "Inizializzazione e finalizzazione"

        /// <summary>
        /// Routine di inizializzazione
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Cursore a croce utilizzato per visualizzare lo spostamento del mouse
                FullCrossCursor.CoordinatesBox = myCoordinatesBox;
                // Cursore a croce generico
                FullCrossCursor.CoordinatesBox = myCoordinatesBox;

                ZoomToDefaultRect();

                // Aggiungo gli handler per la gestione degli eventi di resize della finestra
                AddResizeHandlers();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                //Interaction.MsgBox(ex.Message);
            }
        }

        #region "Funzioni per la gestione della logica di layout"

        /// <summary>
        /// Consente di sospendere temporaneamente la logica di layout per il controllo
        /// </summary>
        private new void SuspendLayout()
        {
            base.SuspendLayout();
            this.myIsLayoutSuspended = true;
        }

        /// <summary>
        /// Consente di riprendere la consueta logica di layout
        /// </summary>
        private new void ResumeLayout()
        {
            base.ResumeLayout();
            this.myIsLayoutSuspended = false;
            // Faccio una Redraw() quando ho concluso l'aggiornamento del layout
            Redraw();
        }

        /// <summary>
        /// Consente di riprendere la consueta logica di layout, imponendo, eventualmente, l'esecuzione di un layout immediato delle richieste di layout in sospeso.
        /// </summary>
        private new void ResumeLayout(bool performLayout)
        {
            base.ResumeLayout(performLayout);
            this.myIsLayoutSuspended = false;
            // Faccio una Redraw() quando ho concluso l'aggiornamento del layout
            Redraw();
        }

        #endregion

        #endregion

        #region "Routine per la scalatura dei Graphics"

        /// <summary>
        /// Ritorna un oggetto graphics derivato dalla bitmap passatagli
        /// L'oggetto ritornato ha le matrici di scalatura e traslazione impostate sul fattore di scala e sulla LogicalOrigin attualmente in uso.
        /// NOTA BENE: E' necessario fare un Dispose() del Graphics ritornato appena si e' finito di utilizzarlo
        /// </summary>
        protected internal Graphics GetScaledGraphicObject(Bitmap Src)
        {
            // Check se la sorgente passatami e' valida
            if (Src == null)
                return null;
            Graphics g = Graphics.FromImage(Src);

            // Creo il Graphics, lo scalo e lo ritorno
            return ScaleGraphicObject(ref g);
        }

        /// <summary>
        /// Ritorna un oggetto graphics con le matrici di scalatura e traslazione impostate 
        /// sul fattore di scala e sulla LogicalOrigin attualmente in uso.
        /// </summary>
        protected internal Graphics ScaleGraphicObject(ref Graphics GR)
        {
            try
            {
                // Check se il Graphics passatomi e' valido
                if (GR == null)
                {
                    return null;
                }

                // Check che il fattore di scala sia valido
                if ((ScaleFactor <= 0.0))
                {
                    //Interaction.MsgBox("Fattore di scala non valido in ScaleGraphicObject()");
                    return GR;
                }

                // Cancello eventuali trasformazioni precedenti
                GR.ResetTransform();

                // Per ottenere una traslazione di coordinate logiche va impostata prima la matrice di scala e poi quella di traslazione 
                GR.ScaleTransform(ScaleFactor, ScaleFactor);
                GR.TranslateTransform(-LogicalOrigin.X, -LogicalOrigin.Y);
                return GR;
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                return null;
            }
        }


        /// <summary>
        /// Ritorna un oggetto graphics con le matrici di scalatura e traslazione impostate 
        /// sul fattore di scala e sulla LogicalOrigin attualmente in uso.
        /// </summary>
        public Graphics GetGraphics()
        {
            try
            {
                Graphics GR = Graphics.FromImage(myRefreshBackBuffer);

                // Check che il fattore di scala sia valido
                if ((ScaleFactor <= 0.0))
                {
                    //Interaction.MsgBox("Fattore di scala non valido in ScaleGraphicObject()");
                    return GR;
                }

                // Cancello eventuali trasformazioni precedenti
                GR.ResetTransform();

                // Per ottenere una traslazione di coordinate logiche va impostata prima la matrice di scala e poi quella di traslazione 
                GR.ScaleTransform(ScaleFactor, ScaleFactor);
                GR.TranslateTransform(-LogicalOrigin.X, -LogicalOrigin.Y);
                return GR;
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.Message);
                LogHelper.GetLogger<ZRGPictureBoxControl>().Error(ex.StackTrace);
                return null;
            }
        }

        #endregion


        #region "Metodi"
        long static_MyDoubleClick_myTimer;
        private bool MyDoubleClick()
        {
            if ((DateTime.Now.Ticks - static_MyDoubleClick_myTimer) < 5000000)
            {
                return true;
            }
            static_MyDoubleClick_myTimer = DateTime.Now.Ticks;
            return false;
        }

        #endregion
    }
}
