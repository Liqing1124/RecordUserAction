using System;
using System.Drawing;
using System.Windows.Forms;

namespace RecordUserAction
{
    public partial class Form1 : Form
    {
        private Button btnRecord;
        private Button btnReplay;
        private Button btnStopReplay; // New button to stop replay
        private Button btnHelp; // New Help button
        private Label lblStatus;
        private Label lblTimer;
        private NumericUpDown numReplayCount; // Added control
        private NumericUpDown numSpeed; // Added control for speed
        private Label lblReplayCount; // Label for replay count
        private Label lblSpeed; // Label for speed control
        private UserActionRecorder actionRecorder;
        private bool isRecording = false;
        private System.Windows.Forms.Timer timerDisplay;
        private DateTime startTime;
        private Point dragStartPosition;
        private bool isDragging = false;

        public Form1()
        {
            InitializeComponent();
            actionRecorder = new UserActionRecorder(this);
            actionRecorder.ReplayStarted += ActionRecorder_ReplayStarted;
            actionRecorder.ReplayCompleted += ActionRecorder_ReplayCompleted;
            this.FormClosing += Form1_FormClosing;
            
            // Add keyboard event handler for hotkey
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        // Panel for card-like container and close button
        private Panel mainPanel;
        // Removed statusPanel as it wasn't used
        // Removed titleLabel
        // Removed closeButton
        private ToolTip toolTip; // Add ToolTip component
        
        private void InitializeComponent()
        {
            this.btnRecord = new Button();
            this.btnReplay = new Button();
            this.btnStopReplay = new Button(); // Initialize stop replay button
            this.btnHelp = new Button(); // Initialize Help button
            this.lblStatus = new Label();
            this.lblTimer = new Label();
            this.numReplayCount = new NumericUpDown(); // Initialize control
            this.numSpeed = new NumericUpDown(); // Initialize control for speed
            this.lblReplayCount = new Label(); // Initialize label for replay count
            this.lblSpeed = new Label(); // Initialize label for speed control
            this.timerDisplay = new System.Windows.Forms.Timer();
            this.mainPanel = new Panel();
            // Removed statusPanel initialization
            // Removed titleLabel initialization
            // Removed closeButton initialization
            this.toolTip = new ToolTip(); // Initialize ToolTip component
            ((System.ComponentModel.ISupportInitialize)(this.numReplayCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.btnRecord.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.System; // Changed FlatStyle
            this.btnRecord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.btnRecord.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.btnRecord.Location = new Point(20, 20); // Adjusted Location
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new Size(150, 40); // Adjusted Size
            // Removed BorderSize = 0
            this.btnRecord.Cursor = Cursors.Hand;
            this.btnRecord.TabIndex = 0;
            this.btnRecord.Anchor = (AnchorStyles.Top | AnchorStyles.Left); // Added Anchor
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new EventHandler(this.btnRecord_Click);
            // 
            // btnReplay
            // 
            this.btnReplay.BackColor = SystemColors.Control; // Standard Background
            // Removed BorderColor
            this.btnReplay.FlatStyle = System.Windows.Forms.FlatStyle.System; // Changed FlatStyle
            this.btnReplay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.btnReplay.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.btnReplay.Location = new Point(180, 20); // Adjusted Location
            this.btnReplay.Name = "btnReplay";
            this.btnReplay.Size = new Size(150, 40); // Adjusted Size
            // Removed BorderSize = 0
            this.btnReplay.Cursor = Cursors.Hand;
            this.btnReplay.TabIndex = 1;
            this.btnReplay.Anchor = (AnchorStyles.Top | AnchorStyles.Left); // Added Anchor
            this.btnReplay.Text = "Replay";
            this.btnReplay.UseVisualStyleBackColor = false;
            this.btnReplay.Click += new EventHandler(this.btnReplay_Click);
            //
            // numReplayCount
            //
            this.numReplayCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numReplayCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.numReplayCount.Location = new Point(410, 75); // Adjusted Location
            this.numReplayCount.Name = "numReplayCount";
            this.numReplayCount.Size = new Size(60, 25); // Adjusted Size
            this.numReplayCount.TabIndex = 2;
            this.numReplayCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numReplayCount.Anchor = (AnchorStyles.Top | AnchorStyles.Right); // Added Anchor
            this.numReplayCount.Minimum = 1; // Must replay at least once
            this.numReplayCount.Value = 1; // Default to 1 replay
            this.numReplayCount.BackColor = System.Drawing.Color.White;
            this.numReplayCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            //
            // numSpeed
            //
            this.numSpeed.DecimalPlaces = 1;
            this.numSpeed.Increment = 0.1M;
            this.numSpeed.Minimum = 0.1M;
            this.numSpeed.Maximum = 10.0M;
            this.numSpeed.Value = 1.0M;
            this.numSpeed.Location = new Point(410, 105); // Adjusted Location
            this.numSpeed.Size = new Size(60, 25); // Adjusted Size
            this.numSpeed.ValueChanged += numSpeed_ValueChanged;
            this.numSpeed.Anchor = (AnchorStyles.Top | AnchorStyles.Right); // Added Anchor
            this.mainPanel.Controls.Add(this.numSpeed);
            //
            // btnStopReplay
            //
            this.btnStopReplay.BackColor = Color.LightCoral; // Adjusted Color
            // Removed BorderColor
            this.btnStopReplay.FlatStyle = System.Windows.Forms.FlatStyle.System; // Changed FlatStyle
            this.btnStopReplay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.btnStopReplay.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.btnStopReplay.Location = new Point(20, 140); // Adjusted Location
            this.btnStopReplay.Name = "btnStopReplay";
            this.btnStopReplay.Size = new Size(450, 40); // Adjusted Size
            this.btnStopReplay.TabIndex = 3;
            this.btnStopReplay.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right); // Added Anchor
            this.btnStopReplay.Text = "Stop Replay";
            this.btnStopReplay.UseVisualStyleBackColor = false;
            this.btnStopReplay.Enabled = false; // Initially disabled
            this.btnStopReplay.FlatAppearance.BorderSize = 0;
            this.btnStopReplay.Cursor = Cursors.Hand;
            this.btnStopReplay.Click += new EventHandler(this.btnStopReplay_Click);
            
            // Set tooltip for Stop Replay button
            this.toolTip.SetToolTip(this.btnStopReplay, "Click to stop replay or press Esc / Ctrl+S");
            
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = SystemColors.Control; // Standard Background
            // Removed BorderColor
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.System; // Changed FlatStyle
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.btnHelp.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.btnHelp.Location = new Point(20, 190); // Adjusted Location
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new Size(450, 35); // Adjusted Size
            this.btnHelp.TabIndex = 4;
            this.btnHelp.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right); // Added Anchor
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.FlatAppearance.BorderSize = 0;
            this.btnHelp.Cursor = Cursors.Hand;
            this.btnHelp.Click += new EventHandler(this.btnHelp_Click);
            
            // Set tooltip for Help button
            this.toolTip.SetToolTip(this.btnHelp, "Click to view help instructions");
            //
            // lblReplayCount
            //
            this.lblReplayCount.AutoSize = true; // Changed AutoSize
            this.lblReplayCount.BackColor = System.Drawing.Color.Transparent;
            this.lblReplayCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblReplayCount.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.lblReplayCount.Location = new Point(340, 77); // Adjusted Location
            this.lblReplayCount.Name = "lblReplayCount";
            // Removed Size
            this.lblReplayCount.Text = "Repeats:";
            this.lblReplayCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblReplayCount.Anchor = (AnchorStyles.Top | AnchorStyles.Right); // Added Anchor
            //
            // lblSpeed
            //
            this.lblSpeed.AutoSize = true; // Changed AutoSize
            this.lblSpeed.BackColor = System.Drawing.Color.Transparent;
            this.lblSpeed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSpeed.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.lblSpeed.Location = new Point(340, 107); // Adjusted Location
            this.lblSpeed.Name = "lblSpeed";
            // Removed Size
            this.lblSpeed.Text = "Speed:";
            this.lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSpeed.Anchor = (AnchorStyles.Top | AnchorStyles.Right); // Added Anchor
            //
            // lblTimer
            // 
            this.lblTimer.AutoSize = false;
            this.lblTimer.BackColor = SystemColors.Control; // Standard Background
            this.lblTimer.BorderStyle = System.Windows.Forms.BorderStyle.None; // Removed Border
            this.lblTimer.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.lblTimer.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.lblTimer.Location = new Point(180, 75); // Adjusted Location
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new Size(150, 50); // Adjusted Size
            this.lblTimer.TabIndex = 4;
            this.lblTimer.Text = "00:00:00";
            this.lblTimer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTimer.Anchor = (AnchorStyles.Top | AnchorStyles.Left); // Added Anchor
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = false;
            this.lblStatus.BackColor = SystemColors.Control; // Standard Background
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point); // Adjusted Font
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlText; // Standard Text Color
            this.lblStatus.Location = new Point(20, 75); // Adjusted Location
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(150, 50); // Adjusted Size
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Anchor = (AnchorStyles.Top | AnchorStyles.Left); // Added Anchor
            // 
            // timerDisplay
            // 
            this.timerDisplay.Interval = 1000; // Update every second
            this.timerDisplay.Tick += new EventHandler(this.timerDisplay_Tick);
            // Removed titleLabel definition
            // Removed closeButton definition
            //
            // mainPanel
            //
            this.mainPanel.BackColor = SystemColors.Control; // Standard Background
            // Removed titleLabel from Controls
            // Removed closeButton from Controls
            this.mainPanel.Controls.Add(this.btnRecord);
            this.mainPanel.Controls.Add(this.btnReplay);
            this.mainPanel.Controls.Add(this.btnStopReplay);
            this.mainPanel.Controls.Add(this.btnHelp);
            this.mainPanel.Controls.Add(this.lblStatus);
            this.mainPanel.Controls.Add(this.lblTimer);
            this.mainPanel.Controls.Add(this.numReplayCount);
            this.mainPanel.Controls.Add(this.lblReplayCount);
            this.mainPanel.Controls.Add(this.numSpeed);
            this.mainPanel.Controls.Add(this.lblSpeed);
            this.mainPanel.Dock = DockStyle.Fill; // Changed Dock style
            this.mainPanel.Location = new Point(0, 0); // Adjusted Location (Dock handles this)
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new Size(500, 250); // Adjusted initial Size
            this.mainPanel.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = SystemColors.Control; // Standard Background
            this.ClientSize = new System.Drawing.Size(500, 250); // Adjusted initial Size
            this.Controls.Add(this.mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable; // Changed FormBorderStyle
            this.MaximizeBox = true; // Enabled MaximizeBox
            this.MinimumSize = new Size(480, 280); // Set Minimum Size
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "User Action Recorder";
            ((System.ComponentModel.ISupportInitialize)(this.numReplayCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout(); // Added PerformLayout
            this.ResumeLayout(false);
        }

        private void timerDisplay_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            lblTimer.Text = elapsed.ToString(@"hh\:mm\:ss");
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                // Start recording
                actionRecorder.StartRecording();
                isRecording = true;
                btnRecord.Text = "Stop Recording";
                lblStatus.Text = "Recording...";
                btnReplay.Enabled = false;
                
                // Start the timer
                startTime = DateTime.Now;
                timerDisplay.Start();
            }
            else
            {
                // Stop recording
                actionRecorder.StopRecording();
                isRecording = false;
                btnRecord.Text = "Record";
                lblStatus.Text = "Recording stopped. Ready to replay.";
                btnReplay.Enabled = true;
                
                // Stop the timer
                timerDisplay.Stop();
            }
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            int replayCount = (int)numReplayCount.Value;
            actionRecorder.ReplaySpeedMultiplier = (float)numSpeed.Value;
            actionRecorder.ReplayActions(replayCount);
        }

        private void ActionRecorder_ReplayStarted(object sender, EventArgs e)
        {
            lblStatus.Text = "Replaying actions...";
            // Disable record and replay buttons during replay
            btnRecord.Enabled = false;
            btnReplay.Enabled = false;
            
            // Enable the stop replay button
            btnStopReplay.Enabled = true;
            
            // Start the timer for replay
            startTime = DateTime.Now;
            timerDisplay.Start();
        }
        
        private void ActionRecorder_ReplayCompleted(object sender, EventArgs e)
        {
            // Stop the display timer
            timerDisplay.Stop();
            
            lblStatus.Text = "Replay complete.";
            btnRecord.Enabled = true;
            btnReplay.Enabled = true;
            btnStopReplay.Enabled = false; // Disable stop button when replay is complete
            
            // Add a small delay to ensure any pending actions are completed
            // This helps prevent accidental button clicks from the replay
            Application.DoEvents();
        }

        private void btnStopReplay_Click(object sender, EventArgs e)
        {
            // Call the StopReplay method to halt the ongoing replay
            actionRecorder.StopReplay();
            lblStatus.Text = "Replay stopped by user.";
        }

        // Removed closeButton_Click handler

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Escape key or Ctrl+S is pressed during replay
            if ((e.KeyCode == Keys.Escape || (e.Control && e.KeyCode == Keys.S)) && actionRecorder.IsReplaying)
            {
                // Call the same method that the Stop Replay button uses
                actionRecorder.StopReplay();
                lblStatus.Text = "Replay stopped by hotkey (" + (e.KeyCode == Keys.Escape ? "Esc" : "Ctrl+S") + ").";
                e.Handled = true; // Mark the event as handled
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up resources
            actionRecorder.Dispose();
        }
        
        private void closeButton_Click(object sender, EventArgs e)
        {
            // Close the application
            this.Close();
        }
        
        // Removed Panel_MouseDown
        // Removed Panel_MouseMove
        // Removed Panel_MouseUp
        
        // Removed OnPaint override
        
        // Removed CreateParams override

        private void numSpeed_ValueChanged(object sender, EventArgs e)
        {
            actionRecorder.ReplaySpeedMultiplier = (float)numSpeed.Value;
        }
        
        private void btnHelp_Click(object sender, EventArgs e)
        {
            // Create and show the help form
            using (HelpForm helpForm = new HelpForm())
            {
                helpForm.ShowDialog(this);
            }
        }
    }
}
