using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Rosatom
{
    public class MainViewViewModel
    {
        public double LenthLine { get; set; }

        private readonly ExternalCommandData _commandData;
        public List<MechanicalSystem> NameSystem { get; } = new List<MechanicalSystem>();
        public MechanicalSystem SelectedSystem { get; set; }
        public DelegateCommand SelectCommand { get; }
        public MainViewViewModel(ExternalCommandData commonData)
        {
            _commandData = commonData;
            NameSystem = Commands.GetMechanicalSystems(commonData);
            SelectCommand = new DelegateCommand(OnSelectCommand);
        }
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectCommand()
        {
            RaiseHideRequest();

            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            double minLength = UnitUtils.Convert(300, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            double maxLength = UnitUtils.Convert(LenthLine, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);


            List<Duct> ducts = Commands.GetDucts(_commandData);
            IEnumerable<Duct> ductFilter = ducts.Where(d => d.MEPSystem.Name.Equals(SelectedSystem.Name));
            using (Transaction tx = new Transaction(doc, "Create breake duct"))
            {
                tx.Start();
                for (int i = 0; i < ductFilter.Count(); i++)
                {
                    Duct duct = ductFilter.ElementAt(i);
                    DuctType typeDuct = duct.DuctType;
                    //Получаем тип соединителя
                    //FamilySymbol nameUnion = typeDuct.Union;

                    LocationCurve ductElement = duct.Location as LocationCurve;
                    Curve curve = ductElement.Curve;

                    double realLenth = curve.Length;

                    bool needsFinalPiece = false;

                    int numOfSplits = (int)Math.Truncate(realLenth / maxLength);
                    if (numOfSplits == 0)
                    {
                        continue;
                    }
                    double finalPiece = realLenth - maxLength * numOfSplits;
                    if (finalPiece < minLength)
                    {
                        needsFinalPiece = true;
                    }
                    XYZ pt0 = curve.GetEndPoint(0);
                    XYZ pt1 = curve.GetEndPoint(1);
                    XYZ vec = pt1.Subtract(pt0).Normalize();
                    double step = 0;

                    for (int j = 0; j < numOfSplits; j++)
                    {
                        if (!(j == numOfSplits - 1 && needsFinalPiece))
                        {
                            step += maxLength;
                        }
                        else
                        {
                            step = step + (realLenth - step) / 2;
                        }
                        XYZ breakPt = pt0.Add(vec.Multiply(step));
                        ElementId newDuctId = MechanicalUtils.BreakCurve(doc, duct.Id, breakPt);
                        Duct newDuct = doc.GetElement(newDuctId) as Duct;

                        Connector con1 = duct.ConnectorManager.Lookup(0);
                        Connector con2 = newDuct.ConnectorManager.Lookup(1);

                        doc.Create.NewUnionFitting(con1, con2);
                    }
                }
                tx.Commit();
            }
            RaiseCloseRequest();
        }

    }
}
