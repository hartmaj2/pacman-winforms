namespace Pacman
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            gameLoopTimer = new System.Windows.Forms.Timer(components);
            startButton = new Button();
            endButton = new Button();
            SuspendLayout();
            // 
            // gameLoopTimer
            // 
            gameLoopTimer.Interval = 1000;
            gameLoopTimer.Tick += gameLoopTimer_Tick;
            // 
            // startButton
            // 
            startButton.Location = new Point(264, 156);
            startButton.Margin = new Padding(4, 2, 4, 2);
            startButton.Name = "startButton";
            startButton.Size = new Size(217, 90);
            startButton.TabIndex = 0;
            startButton.Text = "Start Timer";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_click;
            // 
            // endButton
            // 
            endButton.Location = new Point(264, 67);
            endButton.Name = "endButton";
            endButton.Size = new Size(217, 84);
            endButton.TabIndex = 1;
            endButton.Text = "End Game";
            endButton.UseVisualStyleBackColor = true;
            endButton.Click += endButton_click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(endButton);
            Controls.Add(startButton);
            Margin = new Padding(4, 2, 4, 2);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer gameLoopTimer;
        private Button startButton;
        private Button endButton;
    }
}