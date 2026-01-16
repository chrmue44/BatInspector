using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlSpecialQuestion.xaml
  /// </summary>
  public partial class ctlSpecialQuestion : UserControl
  {
    string[] _relevantFor;
    
    public ctlSpecialQuestion()
    {
      InitializeComponent();
    }


    public int SelectIndex
    {
      get { return _cbSelect.SelectIndex; }
    }

    public void init()
    {
      _cbSelect.SelectIndex = _cbSelect.getItems().Count - 1;
      Visibility = Visibility.Visible;
    }

    public void setup(string label, int index, int widthLbl = 80, int widthTb = 80,
                      dlgSelItemChanged dlgValChange = null, dlgClickLabel dlgClick = null, string tooltip = "", bool edit = true)
    {
      _cbSelect.setup(label, index, widthLbl, widthTb, dlgValChange, dlgClick, tooltip, edit);
    }


    public  void setItems(string[] items, string[] relevantFor)
    { 
      _cbSelect.setItems(items);
      _cbSelect.SelectIndex = items.Length - 1;
      _relevantFor = relevantFor;
      _lblHarmonic.Content = getRelevantForString();
    }


    public void setVisibility(string[] species)
    {
      bool relevant = false;
      foreach (string item in species) 
      {
        SpeciesInfos s = SpeciesInfos.findAbbreviation(item, App.Model.SpeciesInfos);
        relevant |= checkRelevance(s);
        Visibility = relevant ? Visibility.Visible : Visibility.Collapsed;
      }
    }


    private bool checkRelevance(SpeciesInfos s)
    {
      bool retVal = false;
      foreach (string item in _relevantFor)
      {
        if(item == s.Abbreviation)
          retVal = true;
        if(item == s.getGenus())
          retVal = true;
      }
      return retVal;
    }


    private string getRelevantForString()
    {
      string retVal = "";
      foreach (string item in _relevantFor)
      {
        if (retVal == "")
          retVal += item;
        else
          retVal += "; " + item;
      }
      return retVal;
    }
  }
}
