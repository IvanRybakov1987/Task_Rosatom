using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Task_Rosatom
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Filter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Dictionary<string, string> allElement = new Dictionary<string, string>();

            BuiltInCategory ductTerminal = BuiltInCategory.OST_DuctTerminal;
            BuiltInCategory ductAccessory = BuiltInCategory.OST_DuctAccessory;
            BuiltInCategory ductFitting = BuiltInCategory.OST_DuctFitting;
            BuiltInCategory ductCurves = BuiltInCategory.OST_DuctCurves;
            BuiltInCategory flexDuctCurves = BuiltInCategory.OST_FlexDuctCurves;
            BuiltInCategory genericModel = BuiltInCategory.OST_GenericModel;
            BuiltInCategory vechanicalEquipment = BuiltInCategory.OST_MechanicalEquipment;

            List<BuiltInCategory> buildCategory = new List<BuiltInCategory> { ductTerminal, ductAccessory, ductFitting, ductCurves, flexDuctCurves, genericModel, vechanicalEquipment };

            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(ductTerminal));
            categories.Add(new ElementId(ductAccessory));
            categories.Add(new ElementId(ductFitting));
            categories.Add(new ElementId(ductCurves));
            categories.Add(new ElementId(flexDuctCurves));
            categories.Add(new ElementId(genericModel));
            categories.Add(new ElementId(vechanicalEquipment));

            const string nameSystem = "HVAC_KKS system";

            ElementId paramHVAC = new ElementId(857069);

            foreach (var cat in buildCategory)
            {
                List<Element> elementsByCat = new FilteredElementCollector(doc)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType()
                    .ToList();
                foreach (var item in elementsByCat)
                {
                    string paramValue = item.LookupParameter(nameSystem).AsString();
                    string paramName = item.Name;
                    try
                    {
                        allElement.Add(paramValue, paramName);

                    }
                    catch (Exception)
                    {
                    }
                }
                //var distinctList = allElement.Values.Distinct().ToList();
            }


            using (Transaction tx = new Transaction(doc, "Создание фильтров"))
            {
                tx.Start();
                foreach (var item in allElement)
                {
                    ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(doc, nameSystem + " " + item.Key, categories);
                    parameterFilterElement.SetElementFilter(new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(paramHVAC, item.Key, false)));
                }
                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
