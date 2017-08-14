namespace ZRGPictureBox
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.zoomButton1 = new ZRGPictureBox.ZoomButton();
            this.zrgPictureBoxControl1 = new ZRGPictureBox.ZRGPictureBoxControl();
            this.SuspendLayout();
            // 
            // zoomButton1
            // 
            this.zoomButton1.BackColor = System.Drawing.Color.Transparent;
            this.zoomButton1.Dock = System.Windows.Forms.DockStyle.Top;
            this.zoomButton1.LinkedPictureBox = null;
            this.zoomButton1.Location = new System.Drawing.Point(0, 0);
            this.zoomButton1.Name = "zoomButton1";
            this.zoomButton1.Size = new System.Drawing.Size(1143, 39);
            this.zoomButton1.TabIndex = 0;
            // 
            // zrgPictureBoxControl1
            // 
            this.zrgPictureBoxControl1.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.zrgPictureBoxControl1.BackgroundImagePixelSize_Mic = 100;
            this.zrgPictureBoxControl1.ClickAction = ZRGPictureBox.enClickAction.Zoom;
            this.zrgPictureBoxControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.zrgPictureBoxControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zrgPictureBoxControl1.Image = null;
            this.zrgPictureBoxControl1.ImageCustomOrigin = new System.Drawing.Point(0, 0);
            this.zrgPictureBoxControl1.ImagePosition = ZRGPictureBox.enBitmapOriginPosition.TopLeft;
            this.zrgPictureBoxControl1.Location = new System.Drawing.Point(0, 39);
            this.zrgPictureBoxControl1.Name = "zrgPictureBoxControl1";
            this.zrgPictureBoxControl1.Size = new System.Drawing.Size(1143, 588);
            this.zrgPictureBoxControl1.SmartGridAdjust = true;
            this.zrgPictureBoxControl1.TabIndex = 1;
            this.zrgPictureBoxControl1.UnitOfMeasure = ZRGPictureBox.MeasureSystem.enUniMis.mm;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1143, 627);
            this.Controls.Add(this.zrgPictureBoxControl1);
            this.Controls.Add(this.zoomButton1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private ZoomButton zoomButton1;
        private ZRGPictureBoxControl zrgPictureBoxControl1;
    }
}

