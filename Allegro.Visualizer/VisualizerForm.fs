namespace Allegro.Visualizer

open System
open System.Data
open System.Windows.Forms
open System.Drawing
open System.Collections

type VisualizerForm() =
    inherit Form()
    
    [<DefaultValue>] val mutable private grid: CustomDataGridView
    [<DefaultValue>] val mutable private propGrid: PropertyGrid
    [<DefaultValue>] val mutable private splitContainer: SplitContainer
    [<DefaultValue>] val mutable private filterPanel: FlowLayoutPanel
    [<DefaultValue>] val mutable private tableDropDown: ComboBox
    [<DefaultValue>] val mutable private filterTextBox: TextBox
    [<DefaultValue>] val mutable private filterButton: Button
    [<DefaultValue>] val mutable private dataSet: DataSet
    [<DefaultValue>] val mutable private sortColumnsButton: Button
    
    new(caption) as self =
        new VisualizerForm() then            
            self.InitializeComponent()
            self.Text <- caption     

    member private x.InitializeComponent() =
        base.SuspendLayout()

        //base.Text <- caption
        base.ClientSize <- Size(1100, 600)
        base.WindowState <- FormWindowState.Normal
        base.FormBorderStyle <- FormBorderStyle.Sizable
        base.StartPosition <- FormStartPosition.CenterScreen
        base.DoubleBuffered <- true
        base.Margin <- new System.Windows.Forms.Padding(3, 2, 3, 2);

        x.filterPanel <- new FlowLayoutPanel()
        x.filterPanel.FlowDirection <- FlowDirection.LeftToRight
        x.filterPanel.Height <- 30
        x.filterPanel.Dock <- DockStyle.Top

        x.tableDropDown <- new ComboBox()
        x.tableDropDown.DropDownStyle <- ComboBoxStyle.DropDownList
        x.tableDropDown.Width <- 200
        x.tableDropDown.SelectedIndexChanged.AddHandler(new EventHandler(x.TableDropDownSelectedIndexChanged))
        x.filterPanel.Controls.Add(x.tableDropDown)

        x.filterTextBox <- new TextBox()
        x.filterTextBox.Width <- 300
        x.filterPanel.Controls.Add(x.filterTextBox)

        x.filterButton <- new Button()
        x.filterButton.Text <- "Filter"
        x.filterButton.Click.AddHandler(new EventHandler(x.FilterButtonClicked))
        x.filterPanel.Controls.Add(x.filterButton)

        x.propGrid <- new PropertyGrid()
        x.propGrid.Margin <- new System.Windows.Forms.Padding(3, 2, 3, 2);
        x.propGrid.Size <- new System.Drawing.Size(200, 502);
        x.propGrid.Dock <- DockStyle.Fill

        x.grid <- new CustomDataGridView()
        x.grid.Margin <- new System.Windows.Forms.Padding(3, 2, 3, 2);
        x.grid.Dock <- DockStyle.Fill
        x.grid.AllowUserToDeleteRows <- false
        x.grid.AllowUserToAddRows <- false
        x.grid.AllowUserToResizeRows <- false
        x.grid.DataError.AddHandler(fun _ _-> ())

        x.splitContainer <- new SplitContainer()
        (x.splitContainer :> System.ComponentModel.ISupportInitialize).BeginInit()
        x.splitContainer.SuspendLayout()
        x.splitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        x.splitContainer.Panel1.Controls.Add(x.propGrid)
        x.splitContainer.Panel2.Controls.Add(x.grid)
        x.splitContainer.Panel2.Controls.Add(x.filterPanel)
        x.splitContainer.SplitterDistance <- 30
        x.splitContainer.ResumeLayout()
        (x.splitContainer :> System.ComponentModel.ISupportInitialize).EndInit()

        base.Controls.Add(x.splitContainer)
        base.ResumeLayout()

    member x.SetDataSource(dataSource: obj, ?objectType) =
        x.grid.ReadOnly <- false
        match dataSource with
        | :? DataSet as dataSet -> 
            x.dataSet <- dataSet
            x.PopulateTableDropDown(dataSet)              
            x.BindToGrid(dataSet.Tables.[0])
        | :? IDictionary as dictionary ->
            () //TOOD: Dictionary visualizer
        | _ -> 
            x.BindToGrid(dataSource, objectType.Value)
            x.splitContainer.Panel2.Controls.Remove(x.filterPanel)            
        x.grid.ReadOnly <- true
        x.grid.Refresh()

    member x.BindToGrid(selectedDataSource) =
        x.grid.DataSource <- selectedDataSource
        x.propGrid.SelectedObject <- selectedDataSource
        for column in selectedDataSource.Columns do
            let dataGridColumn = x.grid.Columns.[column.ColumnName]
            if dataGridColumn <> null then
                dataGridColumn.ToolTipText <- String.Format("{0}:{1}{2},{3}", column.ColumnName, Environment.NewLine, column.DataType.Name, if column.AllowDBNull then "Null" else "Not Null");
                dataGridColumn.DefaultCellStyle.NullValue <- "(null)";               

    member x.BindToGrid(selectedDataSource, objectType) =
        x.grid.DataSource <- selectedDataSource
        x.propGrid.SelectedObject <- objectType
        for column in x.grid.Columns do
            column.ToolTipText <- String.Format("{0}:{1}{2}", column.Name, Environment.NewLine, column.ValueType.Name);
            column.DefaultCellStyle.NullValue <- "(null)";               

    member private x.PopulateTableDropDown(dataSet) =
        for table in dataSet.Tables do
            x.tableDropDown.Items.Add(table.TableName) |> ignore
        if(x.tableDropDown.Items.Count > 0) then
            x.tableDropDown.SelectedIndex <- 0

    member private x.FilterButtonClicked _ _ =        
        x.grid.ReadOnly <- false
        let dataTable = x.dataSet.Tables.[x.tableDropDown.SelectedIndex]
        if x.filterTextBox.Text.Trim().Length > 0 then
            try 
                let filteredRows = dataTable.Select(x.filterTextBox.Text)
                use filteredDataTable = dataTable.Clone()
                filteredRows |> Array.iter(fun it -> filteredDataTable.ImportRow(it))
                filteredDataTable.AcceptChanges()
                x.BindToGrid(filteredDataTable)
            with
            | e -> MessageBox.Show(e.Message, "Data Filtering", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        else
            x.grid.DataSource <- dataTable
        x.grid.ReadOnly <- true

    member private x.TableDropDownSelectedIndexChanged _ _ =
        let tableName = x.tableDropDown.Items.[x.tableDropDown.SelectedIndex] :?> string
        let dataTable = x.dataSet.Tables.[tableName]
        x.BindToGrid(dataTable)        
