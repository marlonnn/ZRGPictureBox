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
    public partial class ZoomButton : UserControl
    {
        private ZRGPictureBox.ZRGPictureBoxControl myZRGPictureBox;
        public ZRGPictureBox.ZRGPictureBoxControl LinkedPictureBox
        {
            get { return myZRGPictureBox; }
            set
            {
                myZRGPictureBox = value;
                RefreshDisplayButtonState();
            }
        }

        private void RefreshDisplayButtonState()
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    btViewRulers.Checked = LinkedPictureBox.ShowRulers;
                    btViewScrollBars.Checked = LinkedPictureBox.ShowScrollbars;
                    btViewGrid.Checked = LinkedPictureBox.ShowGrid;
                    tbPixelSizeMic.Text = Convert.ToString(LinkedPictureBox.BackgroundImagePixelSize_Mic);
                    btUmDmm.Checked = false;
                    btUmInch.Checked = false;
                    btUmMeters.Checked = false;
                    btUmMicron.Checked = false;
                    btUmMillimeters.Checked = false;
                    switch (LinkedPictureBox.UnitOfMeasure)
                    {
                        case MeasureSystem.enUniMis.dmm:
                            btUmDmm.Checked = true;
                            break;
                        case MeasureSystem.enUniMis.inches:
                            btUmInch.Checked = true;
                            break;
                        case MeasureSystem.enUniMis.meters:
                            btUmMeters.Checked = true;
                            break;
                        case MeasureSystem.enUniMis.micron:
                            btUmMicron.Checked = true;
                            break;
                        case MeasureSystem.enUniMis.mm:
                            btUmMillimeters.Checked = true;
                            break;
                    }
                    switch (LinkedPictureBox.ClickAction)
                    {
                        case enClickAction.MeasureDistance:
                            btMeasure.Checked = true;
                            btZoom.Checked = false;
                            break;
                        case enClickAction.Zoom:
                            btMeasure.Checked = false;
                            btZoom.Checked = true;
                            break;
                        default:
                            btMeasure.Checked = false;
                            btZoom.Checked = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void btZoomFit_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    LinkedPictureBox.ZoomToFit();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }
        private void btMeasure_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    LinkedPictureBox.ClickAction = enClickAction.MeasureDistance;
                    RefreshDisplayButtonState();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }
        private void btZoom_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    LinkedPictureBox.ClickAction = enClickAction.Zoom;
                    RefreshDisplayButtonState();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }
        private void btLoad_Click(object sender, EventArgs e)
        {
            try
            {
                string strFilter = "";
                System.Windows.Forms.OpenFileDialog OpenImageDialog = new System.Windows.Forms.OpenFileDialog();
                OpenImageDialog.Filter = "JPEG File Interchange Format (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphics (*.png)|*.png|Tiff Format(*.tiff)|*.tiff|Graphics Interchange Format (*.gif)|*.gif";
                OpenImageDialog.ShowDialog();
                if (OpenImageDialog.FileName.Length > 0)
                {
                    LinkedPictureBox.Image = System.Drawing.Image.FromFile(OpenImageDialog.FileName);
                    LinkedPictureBox.ZoomToDefaultRect();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void btUmMicron_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox == null)
                    return;

                if (object.ReferenceEquals(sender, btUmDmm))
                {
                    LinkedPictureBox.UnitOfMeasure = MeasureSystem.enUniMis.dmm;
                }
                else if (object.ReferenceEquals(sender, btUmInch))
                {
                    LinkedPictureBox.UnitOfMeasure = MeasureSystem.enUniMis.inches;
                }
                else if (object.ReferenceEquals(sender, btUmMeters))
                {
                    LinkedPictureBox.UnitOfMeasure = MeasureSystem.enUniMis.meters;
                }
                else if (object.ReferenceEquals(sender, btUmMicron))
                {
                    LinkedPictureBox.UnitOfMeasure = MeasureSystem.enUniMis.micron;
                }
                else if (object.ReferenceEquals(sender, btUmMillimeters))
                {
                    LinkedPictureBox.UnitOfMeasure = MeasureSystem.enUniMis.mm;
                }
                LinkedPictureBox.Redraw();
                RefreshDisplayButtonState();
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void tbPixelSizeMic_Click(object sender, EventArgs e)
        {
            //if (LinkedPictureBox != null)
            //{
            //    int newPixelWidth = LinkedPictureBox.BackgroundImagePixelSize_Mic;
            //    string newString = Interaction.InputBox("Insert new pixel width value (micron):", "Pixel width", Convert.ToString(newPixelWidth));
            //    if (newString.Length > 0)
            //    {
            //        newPixelWidth = Convert.ToInt32(newString);
            //        LinkedPictureBox.BackgroundImagePixelSize_Mic = newPixelWidth;
            //        LinkedPictureBox.Redraw(true);
            //        RefreshDisplayButtonState();
            //    }

            //}
        }

        public ZoomButton()
        {
            InitializeComponent();
            this.btLoad.Click += btLoad_Click;
            this.btZoom.Click += btZoom_Click;
            this.btMeasure.Click += btMeasure_Click;
            this.tbPixelSizeMic.Click += tbPixelSizeMic_Click;
            this.btZoomFit.Click += btZoomFit_Click;

        }

        private void btViewRulers_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    btViewRulers.Checked = !(btViewRulers.Checked);
                    LinkedPictureBox.ShowRulers = btViewRulers.Checked;
                    LinkedPictureBox.Redraw();
                    RefreshDisplayButtonState();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void btViewScrollBars_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    btViewScrollBars.Checked = !(btViewScrollBars.Checked);
                    LinkedPictureBox.ShowScrollbars = btViewScrollBars.Checked;
                    LinkedPictureBox.Redraw();
                    RefreshDisplayButtonState();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void btViewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                if (LinkedPictureBox != null)
                {
                    btViewGrid.Checked = !(btViewGrid.Checked);
                    LinkedPictureBox.ShowGrid = btViewGrid.Checked;
                    LinkedPictureBox.Redraw();
                    RefreshDisplayButtonState();
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        private void btUmMicron_Click_1(object sender, EventArgs e)
        {

        }
    }
}
