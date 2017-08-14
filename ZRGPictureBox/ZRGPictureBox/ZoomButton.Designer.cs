using System;

namespace ZRGPictureBox
{
    partial class ZoomButton
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ToolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btLoad = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btZoom = new System.Windows.Forms.ToolStripButton();
            this.btMeasure = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btView = new System.Windows.Forms.ToolStripDropDownButton();
            this.btViewRulers = new System.Windows.Forms.ToolStripMenuItem();
            this.btViewScrollBars = new System.Windows.Forms.ToolStripMenuItem();
            this.btViewGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.btUm = new System.Windows.Forms.ToolStripDropDownButton();
            this.btUmMicron = new System.Windows.Forms.ToolStripMenuItem();
            this.btUmDmm = new System.Windows.Forms.ToolStripMenuItem();
            this.btUmMillimeters = new System.Windows.Forms.ToolStripMenuItem();
            this.btUmInch = new System.Windows.Forms.ToolStripMenuItem();
            this.btUmMeters = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbPixelSizeMic = new System.Windows.Forms.ToolStripLabel();
            this.btZoomFit = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStrip1
            // 
            this.ToolStrip1.AutoSize = false;
            this.ToolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.ToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btLoad,
            this.ToolStripSeparator2,
            this.btZoom,
            this.btMeasure,
            this.ToolStripSeparator1,
            this.btView,
            this.btUm,
            this.ToolStripSeparator3,
            this.ToolStripLabel1,
            this.tbPixelSizeMic,
            this.btZoomFit});
            this.ToolStrip1.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip1.Name = "ToolStrip1";
            this.ToolStrip1.Size = new System.Drawing.Size(552, 39);
            this.ToolStrip1.TabIndex = 78;
            this.ToolStrip1.Text = "ToolStrip1";
            // 
            // btLoad
            // 
            this.btLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btLoad.Image = global::ZRGPictureBox.Properties.Resources.open;
            this.btLoad.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(36, 36);
            this.btLoad.Text = "ToolStripButton2";
            this.btLoad.ToolTipText = "Load Image";
            // 
            // ToolStripSeparator2
            // 
            this.ToolStripSeparator2.Name = "ToolStripSeparator2";
            this.ToolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // btZoom
            // 
            this.btZoom.Checked = true;
            this.btZoom.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btZoom.Image = global::ZRGPictureBox.Properties.Resources.zoom;
            this.btZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btZoom.Name = "btZoom";
            this.btZoom.Size = new System.Drawing.Size(36, 36);
            this.btZoom.Text = "ToolStripButton1";
            this.btZoom.ToolTipText = "Zoom mode";
            // 
            // btMeasure
            // 
            this.btMeasure.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btMeasure.Image = global::ZRGPictureBox.Properties.Resources.measure;
            this.btMeasure.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btMeasure.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btMeasure.Name = "btMeasure";
            this.btMeasure.Size = new System.Drawing.Size(36, 36);
            this.btMeasure.Text = "ToolStripButton2";
            this.btMeasure.ToolTipText = "Gauging mode";
            // 
            // ToolStripSeparator1
            // 
            this.ToolStripSeparator1.Name = "ToolStripSeparator1";
            this.ToolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // btView
            // 
            this.btView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btViewRulers,
            this.btViewScrollBars,
            this.btViewGrid});
            this.btView.Image = global::ZRGPictureBox.Properties.Resources.view;
            this.btView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btView.Name = "btView";
            this.btView.Size = new System.Drawing.Size(45, 36);
            this.btView.Text = "ToolStripDropDownButton1";
            this.btView.ToolTipText = "Visible items";
            // 
            // btViewRulers
            // 
            this.btViewRulers.Checked = true;
            this.btViewRulers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btViewRulers.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btViewRulers.Name = "btViewRulers";
            this.btViewRulers.Size = new System.Drawing.Size(138, 22);
            this.btViewRulers.Text = "Rulers";
            this.btViewRulers.Click += new System.EventHandler(this.btViewRulers_Click);
            // 
            // btViewScrollBars
            // 
            this.btViewScrollBars.Checked = true;
            this.btViewScrollBars.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btViewScrollBars.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btViewScrollBars.Name = "btViewScrollBars";
            this.btViewScrollBars.Size = new System.Drawing.Size(138, 22);
            this.btViewScrollBars.Text = "Scroll bars";
            this.btViewScrollBars.Click += new System.EventHandler(this.btViewScrollBars_Click);
            // 
            // btViewGrid
            // 
            this.btViewGrid.Checked = true;
            this.btViewGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btViewGrid.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btViewGrid.Name = "btViewGrid";
            this.btViewGrid.Size = new System.Drawing.Size(138, 22);
            this.btViewGrid.Text = "Grid";
            this.btViewGrid.Click += new System.EventHandler(this.btViewGrid_Click);
            // 
            // btUm
            // 
            this.btUm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btUm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btUmMicron,
            this.btUmDmm,
            this.btUmMillimeters,
            this.btUmInch,
            this.btUmMeters});
            this.btUm.Image = global::ZRGPictureBox.Properties.Resources.um;
            this.btUm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btUm.Name = "btUm";
            this.btUm.Size = new System.Drawing.Size(45, 36);
            this.btUm.Text = "ToolStripDropDownButton1";
            this.btUm.ToolTipText = "Measure unit";
            // 
            // btUmMicron
            // 
            this.btUmMicron.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUmMicron.Name = "btUmMicron";
            this.btUmMicron.Size = new System.Drawing.Size(117, 22);
            this.btUmMicron.Text = "micron";
            this.btUmMicron.Click += new System.EventHandler(this.btUmMicron_Click);
            // 
            // btUmDmm
            // 
            this.btUmDmm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUmDmm.Name = "btUmDmm";
            this.btUmDmm.Size = new System.Drawing.Size(117, 22);
            this.btUmDmm.Text = "mm/10";
            this.btUmDmm.Click += new System.EventHandler(this.btUmMicron_Click);
            // 
            // btUmMillimeters
            // 
            this.btUmMillimeters.Checked = true;
            this.btUmMillimeters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btUmMillimeters.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUmMillimeters.Name = "btUmMillimeters";
            this.btUmMillimeters.Size = new System.Drawing.Size(117, 22);
            this.btUmMillimeters.Text = "mm";
            this.btUmMillimeters.Click += new System.EventHandler(this.btUmMicron_Click);
            // 
            // btUmInch
            // 
            this.btUmInch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUmInch.Name = "btUmInch";
            this.btUmInch.Size = new System.Drawing.Size(117, 22);
            this.btUmInch.Text = "inches";
            this.btUmInch.Click += new System.EventHandler(this.btUmMicron_Click);
            // 
            // btUmMeters
            // 
            this.btUmMeters.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btUmMeters.Name = "btUmMeters";
            this.btUmMeters.Size = new System.Drawing.Size(117, 22);
            this.btUmMeters.Text = "meters";
            this.btUmMeters.Click += new System.EventHandler(this.btUmMicron_Click);
            // 
            // ToolStripSeparator3
            // 
            this.ToolStripSeparator3.Name = "ToolStripSeparator3";
            this.ToolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // ToolStripLabel1
            // 
            this.ToolStripLabel1.Name = "ToolStripLabel1";
            this.ToolStripLabel1.Size = new System.Drawing.Size(115, 36);
            this.ToolStripLabel1.Text = "Pixel size (micron):";
            // 
            // tbPixelSizeMic
            // 
            this.tbPixelSizeMic.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPixelSizeMic.ForeColor = System.Drawing.Color.DarkBlue;
            this.tbPixelSizeMic.Name = "tbPixelSizeMic";
            this.tbPixelSizeMic.Size = new System.Drawing.Size(37, 36);
            this.tbPixelSizeMic.Text = "100";
            this.tbPixelSizeMic.ToolTipText = "Click to change";
            // 
            // btZoomFit
            // 
            this.btZoomFit.Checked = true;
            this.btZoomFit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btZoomFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btZoomFit.Image = global::ZRGPictureBox.Properties.Resources.zoomFit;
            this.btZoomFit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btZoomFit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btZoomFit.Name = "btZoomFit";
            this.btZoomFit.Size = new System.Drawing.Size(36, 36);
            this.btZoomFit.Text = "ToolStripButton1";
            this.btZoomFit.ToolTipText = "Fit image";
            // 
            // ZoomButton
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.ToolStrip1);
            this.Name = "ZoomButton";
            this.Size = new System.Drawing.Size(552, 39);
            this.ToolStrip1.ResumeLayout(false);
            this.ToolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.ToolStrip ToolStrip1;
        private System.Windows.Forms.ToolStripButton btMeasure;
        private System.Windows.Forms.ToolStripButton btZoom;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton btView;
        private System.Windows.Forms.ToolStripMenuItem btViewRulers;
        private System.Windows.Forms.ToolStripMenuItem btViewScrollBars;
        private System.Windows.Forms.ToolStripMenuItem btViewGrid;
        private System.Windows.Forms.ToolStripButton btLoad;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton btUm;
        private System.Windows.Forms.ToolStripMenuItem btUmMicron;
        private System.Windows.Forms.ToolStripMenuItem btUmMillimeters;
        private System.Windows.Forms.ToolStripMenuItem btUmInch;
        private System.Windows.Forms.ToolStripMenuItem btUmMeters;
        private System.Windows.Forms.ToolStripMenuItem btUmDmm;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel ToolStripLabel1;
        private System.Windows.Forms.ToolStripLabel tbPixelSizeMic;
        private System.Windows.Forms.ToolStripButton btZoomFit;
        #endregion
    }
}
