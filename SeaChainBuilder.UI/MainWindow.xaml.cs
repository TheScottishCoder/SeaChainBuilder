using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeaChainBuilder.UI

{   /// Useful Notes, Yay!
    /// A lot of try catches in order to quickly handle crashes
    /// UniformGrid doesn't use vectors
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ColorList();
        }

        public Data _dataMap = new Data();
        public List<Texture> textures = new List<Texture>();
        public int totalTiles = 0;
        public bool brushPlus = false;
        public List<Tag> tags = new List<Tag>();
        public Vector currentPos = new Vector();

        private void Btn_CreateGrid(object sender, RoutedEventArgs e)
        {
            UniformGrid.Rows = Convert.ToInt32(txt_Height.Text);
            UniformGrid.Columns = Convert.ToInt32(txt_Width.Text);


            _dataMap.height = Convert.ToInt32(txt_Height.Text);
            _dataMap.width = Convert.ToInt32(txt_Width.Text);

            totalTiles = _dataMap.height * _dataMap.width;

            for(int i = 0; i < totalTiles; i++)
            {
                UniformGrid.Children.Add(new Canvas());
            }

            //txt_Height.Text = "Height";
            //txt_Width.Text = "Width";

            string[,] t = new string[_dataMap.height, _dataMap.width];

            for (int y = 0; y < _dataMap.height; y++)
            {
                for (int x = 0; x < _dataMap.width; x++)
                {
                    t[x, y] = "\\";
                }
            }

            _dataMap._map = t;
        }
        private void Btn_Test(object sender, RoutedEventArgs e) // this is the method for saving
        {
            string temp = "";

            for(int y = 0; y < _dataMap._map.GetLength(0); y++)
            {
                for(int x = 0; x <_dataMap._map.GetLength(1); x++)
                {
                    temp += _dataMap._map[x, y];
                }

                temp += "\n";
            }

            File.WriteAllText("map.txt", temp);

            string serializedJson = JsonConvert.SerializeObject(tags, Formatting.Indented);

            File.WriteAllText("tags.txt", serializedJson);
        }
        private void Btn_AddTexture(object sender, RoutedEventArgs e)
        {
            Texture t = new Texture();
            t.Name = txt_Texture.Text;
            t.ID = txt_TextureID.Text;

            Type brushesType = typeof(Brushes);
            var properties = brushesType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach (var prop in properties)
            {
                SolidColorBrush brush = (SolidColorBrush)prop.GetValue(null, null);

                if (prop.Name == list_Colors.SelectedItems[0].ToString())
                {
                    t.Color = brush;
                    break;
                }
            }

            txt_Texture.Text = "Texture Name";
            txt_TextureID.Text = "Texture ID";

            foreach(var item in textures)
            {
                if(t.ID == item.ID)
                {
                    MessageBox.Show("A Texture already contains this ID!", "Error");
                    return;
                }
            }

            textures.Add(t);
            UpdateTextureList();
        }
        private void UpdateTextureList()
        {
            list_Textures.Items.Clear();

            foreach(var item in textures)
            {
                list_Textures.Items.Add(item.Name + " : " + item.ID + " : " + item.Color.ToString());
            }
        }
        private void TextureListKeyEvent(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                textures.RemoveAt(list_Textures.SelectedIndex);
                UpdateTextureList();
            }
        }
        private void ColorList()
        {
            Type brushesType = typeof(Brushes);
            var properties = brushesType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach(var prop in properties)
            {
                string name = prop.Name;
                SolidColorBrush brush = (SolidColorBrush)prop.GetValue(null, null);

                list_Colors.Items.Add(name);
            }
        }
        private void UpdateCanvasEvent(object sender, SelectionChangedEventArgs e)
        {
            Type brushesType = typeof(Brushes);
            var properties = brushesType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach (var prop in properties)
            {
                SolidColorBrush brush = (SolidColorBrush)prop.GetValue(null, null);

                if (prop.Name == e.AddedItems[0].ToString())
                {
                    color_display.Background = brush;
                    break;
                }
            }
        }
        private void GridClickEvent(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (list_Textures.SelectedIndex == -1 || UniformGrid.Children.Count <= 0)
                    return;
                
                if (brushPlus == false)
                {
                    Vector pos = GetGridVector();
                    int rPos = Vec_to_int(pos);

                    var tile = UniformGrid.Children[rPos] as Canvas;
                    tile.Background = textures[list_Textures.SelectedIndex].Color;

                    _dataMap._map[Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y)] = textures[list_Textures.SelectedIndex].ID;
                }
                else
                { // if brush plus is enabled do a 3x3, find a better way of doing this. It's just dirty.
                    Vector pos = GetGridVector();
                    List<Vector> positions = new List<Vector>();
                    positions.Add(pos);
                    positions.Add(new Vector((pos.X - 1), (pos.Y - 1)));
                    positions.Add(new Vector((pos.X), (pos.Y - 1)));
                    positions.Add(new Vector((pos.X + 1), (pos.Y - 1)));
                    positions.Add(new Vector((pos.X - 1), (pos.Y)));
                    positions.Add(new Vector((pos.X + 1), (pos.Y)));
                    positions.Add(new Vector((pos.X - 1), (pos.Y + 1)));
                    positions.Add(new Vector((pos.X), (pos.Y + 1)));
                    positions.Add(new Vector((pos.X + 1), (pos.Y + 1)));

                    foreach (var item in positions)
                    {
                        try
                        {
                            var tile = UniformGrid.Children[Vec_to_int(item)] as Canvas;
                            tile.Background = textures[list_Textures.SelectedIndex].Color;

                            _dataMap._map[Convert.ToInt32(item.X), Convert.ToInt32(item.Y)] = textures[list_Textures.SelectedIndex].ID;
                        }
                        catch { }
                    }
                }
            }

            if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                currentPos = GetGridVector();
                txt_Position.Text = "X: " + currentPos.X + " Y: " + currentPos.Y;
            }
        }
        private Vector GetGridVector()
        {
            var point = Mouse.GetPosition(UniformGrid);

            int y = 0;
            int x = 0;

            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;

            for (int i = 0; i < _dataMap.height; i++){
                accumulatedHeight += (UniformGrid.Height / _dataMap.height);
                if (accumulatedHeight >= point.Y)
                    break;
                y++;
            }
            
            for (int i = 0; i < _dataMap.width; i++){
                accumulatedWidth += (UniformGrid.Width / _dataMap.width);
                if (accumulatedWidth >= point.X)
                    break;
                x++;
            }

            return new Vector(x, y);
        }
        private void Btn_FillSheet(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to fill the entire canvas? \nThis will overwrite current tiles", "Warning!", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                for (int i = 0; i < totalTiles; i++)
                {
                    try
                    {
                        var tile = UniformGrid.Children[i] as Canvas;
                        tile.Background = textures[list_Textures.SelectedIndex].Color;                        
                    }
                    catch { }
                }

                for(int y = 0; y < _dataMap.height; y++)
                {
                    for(int x = 0; x < _dataMap.width; x++)
                    {
                        _dataMap._map[x, y] = textures[list_Textures.SelectedIndex].ID;
                    }
                }
            }
        }
        private Vector Int_to_Vec(int i)
        {
                int x = (i - 1) % 9;
                int y = (i - 1) / 9;

                return new Vector(x, y);
        }
        private int Vec_to_int(Vector v)
        {
            return Convert.ToInt32((v.Y * _dataMap.width) + v.X);
        }
        private void Btn_IncreaseBrush(object sender, RoutedEventArgs e) => brushPlus = true;
        private void Btn_DecreaseBrush(object sender, RoutedEventArgs e) => brushPlus = false;

        private void Btn_AddTag(object sender, RoutedEventArgs e)
        {
            bool found = false;

            foreach(var tag in tags)
            {
                if (tag.Position == currentPos)
                {
                    if (!tag.tags.Contains(txt_Tag.Text))
                    {
                        tag.tags.Add(txt_Tag.Text);
                        found = true;
                    }
                }
            }

            if(found == false)
            {
                CreateTag();
                Btn_AddTag(null, null);
            }
        }

        private void CreateTag()
        {
            Tag t = new Tag();
            t.Position = currentPos;

            tags.Add(t);
        }
    }
}
