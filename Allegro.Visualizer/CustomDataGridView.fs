namespace Allegro.Visualizer

type public CustomDataGridView() as self =
    inherit System.Windows.Forms.DataGridView()

    do
        self.DoubleBuffered <- true