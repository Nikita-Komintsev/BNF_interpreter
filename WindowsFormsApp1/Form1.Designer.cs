namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
      this.text_Out = new System.Windows.Forms.TextBox();
      this.text_In = new System.Windows.Forms.RichTextBox();
      this.text_numeric = new System.Windows.Forms.RichTextBox();
      this.BNF_text = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // text_Out
      // 
      this.text_Out.Location = new System.Drawing.Point(2, 624);
      this.text_Out.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.text_Out.Multiline = true;
      this.text_Out.Name = "text_Out";
      this.text_Out.ReadOnly = true;
      this.text_Out.Size = new System.Drawing.Size(1225, 149);
      this.text_Out.TabIndex = 1;
      // 
      // text_In
      // 
      this.text_In.Location = new System.Drawing.Point(53, 15);
      this.text_In.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.text_In.Name = "text_In";
      this.text_In.Size = new System.Drawing.Size(499, 519);
      this.text_In.TabIndex = 2;
      this.text_In.Text = "";
      this.text_In.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.text_In_ContentsResized);
      this.text_In.VScroll += new System.EventHandler(this.text_In_VScroll);
      this.text_In.MouseDown += new System.Windows.Forms.MouseEventHandler(this.code_TextBox_MouseDown);
      // 
      // text_numeric
      // 
      this.text_numeric.Location = new System.Drawing.Point(14, 15);
      this.text_numeric.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.text_numeric.Name = "text_numeric";
      this.text_numeric.ReadOnly = true;
      this.text_numeric.Size = new System.Drawing.Size(32, 519);
      this.text_numeric.TabIndex = 3;
      this.text_numeric.Text = "";
      // 
      // BNF_text
      // 
      this.BNF_text.Location = new System.Drawing.Point(575, 15);
      this.BNF_text.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.BNF_text.Multiline = true;
      this.BNF_text.Name = "BNF_text";
      this.BNF_text.ReadOnly = true;
      this.BNF_text.Size = new System.Drawing.Size(652, 519);
      this.BNF_text.TabIndex = 4;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(818, 542);
      this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(122, 44);
      this.button1.TabIndex = 5;
      this.button1.Text = "Выполнить";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1239, 778);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.BNF_text);
      this.Controls.Add(this.text_numeric);
      this.Controls.Add(this.text_In);
      this.Controls.Add(this.text_Out);
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "Form1";
      this.Text = "Form1";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox text_Out;
        private System.Windows.Forms.RichTextBox text_In;
        private System.Windows.Forms.RichTextBox text_numeric;
        private System.Windows.Forms.TextBox BNF_text;
        private System.Windows.Forms.Button button1;
    }
}

