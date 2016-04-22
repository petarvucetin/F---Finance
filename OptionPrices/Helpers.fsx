#r "System.Windows.Forms"
#r "System.Drawing"
#r @"WindowsBase.dll"
#r @"PresentationCore.dll"
#r @"PresentationFramework.dll"

open System.Drawing
open System.Windows
open System.Windows.Forms
open System.Windows.Controls

module TryParser =
    // convenient, functional TryParse wrappers returning option<'a>
    let tryParseWith tryParseFunc = tryParseFunc >> function
        | true, v    -> Some v
        | false, _   -> None

    let parseDate   = tryParseWith System.DateTime.TryParse
    let parseInt    = tryParseWith System.Int32.TryParse
    let parseSingle = tryParseWith System.Single.TryParse
    let parseDouble = tryParseWith System.Double.TryParse
    // etc.

    // active patterns for try-parsing strings
    let (|Date|_|)   = parseDate
    let (|Int|_|)    = parseInt
    let (|Single|_|) = parseSingle
    let (|Double|_|) = parseDouble



type Wrapper(s:obj) = 
  member x.Value = s.ToString()

let grid<'T> (x:seq<'T>) =     
  let defaultFont = new Font( "Consolas", 16.0f)
  let form = new Form(Visible = true)    
  let data = new DataGridView(Dock = DockStyle.Fill)
  data.DefaultCellStyle.Font <- defaultFont
  form.Controls.Add(data)
  data.AutoGenerateColumns <- true
  if typeof<'T>.IsPrimitive || typeof<'T> = typeof<string> then
    data.DataSource <- [| for v in x -> Wrapper(box v) |]
  else 
    data.DataSource <- x |> Seq.toArray

let  wpfGrid<'T> (x:seq<'T>) = 
    let win = new Window(Title="Test DataGrid")
    win.FontSize <- 16.0
    win.FontFamily <- new Media.FontFamily("Consolas")
    let datagrid = DataGrid()
    datagrid.HeadersVisibility <- DataGridHeadersVisibility.Column
    datagrid.ItemsSource <- x |> Seq.toArray
    win.Content <- new ScrollViewer(Content=datagrid)
    win.Show()