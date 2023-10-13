namespace PdfUtil.WPFUI.Utils
{
    class SquaredPosition
    {
        int col = 0;
        int row = 0;

        public SquaredPosition(int row, int col)
        {
            this.col = col;
            this.row = row;
        }

        public int Col
        {
            get { return this.col; }
        }

        public int Row
        {
            get { return this.row; }
        }
    }
}
