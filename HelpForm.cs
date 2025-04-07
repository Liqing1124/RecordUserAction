using System;
using System.Drawing;
using System.Windows.Forms;

namespace RecordUserAction
{
    public partial class HelpForm : Form
    {
        private Panel mainPanel;
        private Label titleLabel;
        private Button closeButton;
        private RichTextBox helpTextBox;
        private Point dragStartPosition;
        private bool isDragging = false;

        public HelpForm()
        {
            InitializeComponent();
            
            // Add mouse event handlers for dragging the form
            this.mainPanel.MouseDown += Panel_MouseDown;
            this.mainPanel.MouseMove += Panel_MouseMove;
            this.mainPanel.MouseUp += Panel_MouseUp;
            this.titleLabel.MouseDown += Panel_MouseDown;
            this.titleLabel.MouseMove += Panel_MouseMove;
            this.titleLabel.MouseUp += Panel_MouseUp;
        }

        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.titleLabel = new Label();
            this.closeButton = new Button();
            this.helpTextBox = new RichTextBox();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.White;
            this.mainPanel.Controls.Add(this.titleLabel);
            this.mainPanel.Controls.Add(this.closeButton);
            this.mainPanel.Controls.Add(this.helpTextBox);
            this.mainPanel.Location = new Point(12, 12);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new Size(576, 476);
            this.mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = false;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.titleLabel.Location = new Point(20, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new Size(350, 40);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Help - User Action Recorder";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.Transparent;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.closeButton.Location = new Point(516, 15);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(40, 40);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "×";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new EventHandler(this.closeButton_Click);
            // 
            // helpTextBox
            // 
            this.helpTextBox.BackColor = System.Drawing.Color.White;
            this.helpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.helpTextBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.helpTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.helpTextBox.Location = new Point(20, 70);
            this.helpTextBox.Name = "helpTextBox";
            this.helpTextBox.ReadOnly = true;
            this.helpTextBox.Size = new Size(536, 386);
            this.helpTextBox.TabIndex = 2;
            this.helpTextBox.Text = "How to Use User Action Recorder\n\n" +
                "1. Recording Actions:\n" +
                "   • Click the 'Record' button to start recording your mouse and keyboard actions.\n" +
                "   • Perform the actions you want to automate.\n" +
                "   • Click 'Stop Recording' when finished.\n\n" +
                "2. Replaying Actions:\n" +
                "   • After recording, click the 'Replay' button to replay your actions.\n" +
                "   • Use the numeric control to set how many times to repeat the actions.\n" +
                "   • Adjust the speed control to make the replay faster or slower.\n\n" +
                "3. Stopping Replay:\n" +
                "   • Click the 'Stop Replay' button to halt an ongoing replay.\n" +
                "   • You can also press the Esc key or Ctrl+S to stop the replay.\n\n" +
                "4. Tips for Best Results:\n" +
                "   • For more precise replays, avoid moving the mouse during recording except when necessary.\n" +
                "   • Keep the application window visible during replay.\n" +
                "   • If the replay seems off, try adjusting the speed multiplier.\n\n" +
                "5. Troubleshooting:\n" +
                "   • If replay doesn't work as expected, try recording again with simpler actions.\n" +
                "   • Make sure the target application is in the same state as when you recorded.";
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this.mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "HelpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Help - User Action Recorder";
            this.mainPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            // Close the form
            this.Close();
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            // Only start dragging on left mouse button
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPosition = e.Location;
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate the new position of the form
                Point newLocation = this.Location;
                newLocation.X += e.X - dragStartPosition.X;
                newLocation.Y += e.Y - dragStartPosition.Y;
                this.Location = newLocation;
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop dragging
            isDragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Create rounded corners for the form
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            int radius = 20;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            this.Region = new Region(path);
            
            // Create rounded corners for the main panel
            if (mainPanel != null)
            {
                mainPanel.Paint += (s, pe) => {
                    System.Drawing.Drawing2D.GraphicsPath panelPath = new System.Drawing.Drawing2D.GraphicsPath();
                    int panelRadius = 15;
                    Rectangle panelRect = new Rectangle(0, 0, mainPanel.Width, mainPanel.Height);
                    panelPath.AddArc(panelRect.X, panelRect.Y, panelRadius, panelRadius, 180, 90);
                    panelPath.AddArc(panelRect.Right - panelRadius, panelRect.Y, panelRadius, panelRadius, 270, 90);
                    panelPath.AddArc(panelRect.Right - panelRadius, panelRect.Bottom - panelRadius, panelRadius, panelRadius, 0, 90);
                    panelPath.AddArc(panelRect.X, panelRect.Bottom - panelRadius, panelRadius, panelRadius, 90, 90);
                    panelPath.CloseAllFigures();
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    pe.Graphics.DrawPath(new Pen(Color.FromArgb(220, 220, 220), 1), panelPath);
                };
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                return cp;
            }
        }
    }
}