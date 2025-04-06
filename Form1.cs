using System;
using System.Drawing;
using System.Windows.Forms;

namespace RecordUserAction
{
    public partial class Form1 : Form
    {
        private Button btnRecord;
        private Button btnReplay;
        private Label lblStatus;
        private Label lblTimer;
        private NumericUpDown numReplayCount; // Added control
        private UserActionRecorder actionRecorder;
        private bool isRecording = false;
        private System.Windows.Forms.Timer timerDisplay;
        private DateTime startTime;

        public Form1()
        {
            InitializeComponent();
            actionRecorder = new UserActionRecorder(this);
            actionRecorder.ReplayStarted += ActionRecorder_ReplayStarted;
            actionRecorder.ReplayCompleted += ActionRecorder_ReplayCompleted;
            this.FormClosing += Form1_FormClosing;
        }

        private void InitializeComponent()
        {
            this.btnRecord = new Button();
            this.btnReplay = new Button();
            this.lblStatus = new Label();
            this.lblTimer = new Label();
            this.numReplayCount = new NumericUpDown(); // Initialize control
            this.timerDisplay = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // btnRecord
            // 
            this.btnRecord.Location = new Point(30, 30);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new Size(120, 40);
            this.btnRecord.TabIndex = 0;
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new EventHandler(this.btnRecord_Click);
            // 
            // btnReplay
            // 
            this.btnReplay.Location = new Point(170, 30);
            this.btnReplay.Name = "btnReplay";
            this.btnReplay.Size = new Size(120, 40);
            this.btnReplay.TabIndex = 1;
            this.btnReplay.Text = "Replay";
            this.btnReplay.UseVisualStyleBackColor = true;
            this.btnReplay.Click += new EventHandler(this.btnReplay_Click);
            //
            // numReplayCount
            //
            this.numReplayCount.Location = new Point(310, 38); // Position near Replay button
            this.numReplayCount.Name = "numReplayCount";
            this.numReplayCount.Size = new Size(60, 23);
            this.numReplayCount.TabIndex = 4; // Next tab index
            this.numReplayCount.Minimum = 1; // Must replay at least once
            this.numReplayCount.Value = 1; // Default to 1 replay
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(30, 90);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(200, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready";
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Location = new Point(250, 90);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new Size(100, 20);
            this.lblTimer.TabIndex = 3;
            this.lblTimer.Text = "00:00:00";
            // 
            // timerDisplay
            // 
            this.timerDisplay.Interval = 1000; // Update every second
            this.timerDisplay.Tick += new EventHandler(this.timerDisplay_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 150); // Adjusted size slightly if needed, but seems ok
            this.Controls.Add(this.numReplayCount); // Add control to form
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnReplay);
            this.Controls.Add(this.btnRecord);
            this.Name = "Form1";
            this.Text = "User Action Recorder";
            this.ResumeLayout(false);
            this.PerformLayout();
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
            // Start replaying in a separate thread with the specified count
            actionRecorder.ReplayActions(replayCount);
        }

        private void ActionRecorder_ReplayStarted(object sender, EventArgs e)
        {
            lblStatus.Text = "Replaying actions...";
            // Disable all buttons during replay
            btnRecord.Enabled = false;
            btnReplay.Enabled = false;
            
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
            
            // Add a small delay to ensure any pending actions are completed
            // This helps prevent accidental button clicks from the replay
            Application.DoEvents();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up resources
            actionRecorder.Dispose();
        }
    }
}
