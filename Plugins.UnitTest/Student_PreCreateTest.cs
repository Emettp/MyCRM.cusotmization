using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using dotnetlittleboy.cusotmization.Plugins;
namespace Plugins.UnitTest
{
    [TestClass]
    public class Student_PreCreateTest
    {
        [TestMethod]
        public void CorrectAutoNumber()
        {
            var fakeContext = new XrmFakedContext();
            var service = fakeContext.GetOrganizationService();

            //Create Target Entity
            var targetEntity = new Entity("dnlb_student");
            var targetId = Guid.NewGuid();
            targetEntity.Id = targetId;

            //Create input parameter
            var inputParameter = new ParameterCollection();
            inputParameter.Add("Target", targetEntity);

            //create plugin executoin context
            var fakePluginExecutionContext = new XrmFakedPluginExecutionContext()
            {
                MessageName = "Create",
                Stage = 20,
                UserId = Guid.NewGuid(),
                PrimaryEntityName = "dnlb_student",
                PrimaryEntityId = targetId,
                InputParameters = inputParameter
            };

            //create auto number dummy record
            var entityStudentAutoNumber = new Entity("dnlb_autonumberconfiguration");
            entityStudentAutoNumber.Attributes.Add("dnlb_prefix", "IND");
            entityStudentAutoNumber.Attributes.Add("dnlb_suffix", "DNLB");
            entityStudentAutoNumber.Attributes.Add("dnlb_seperator", "-");
            entityStudentAutoNumber.Attributes.Add("dnlb_currentnumber", "000002");
            entityStudentAutoNumber.Attributes.Add("dnlb_name", "studentautonumber");
            service.Create(entityStudentAutoNumber);

            //Execute the plugin code
            fakeContext.ExecutePluginWith<Student_PreCreate>(fakePluginExecutionContext);
            Assert.AreEqual(targetEntity.GetAttributeValue<string>("dnlb_name"), "IND-20201221-DNLB-000003");
        }

        [TestMethod]
        public void TargetEntityIsNotCorrect()
        {
            var fakeContext = new XrmFakedContext();
            var service = fakeContext.GetOrganizationService();

            //Create Target Entity
            var targetEntity = new Entity("dnlb_abc");
            var targetId = Guid.NewGuid();
            targetEntity.Id = targetId;

            //Create input parameter
            var inputParameter = new ParameterCollection();
            inputParameter.Add("Target", targetEntity);

            //create plugin executoin context
            var fakePluginExecutionContext = new XrmFakedPluginExecutionContext()
            {
                MessageName = "Create",
                Stage = 20,
                UserId = Guid.NewGuid(),
                PrimaryEntityName = "dnlb_student",
                PrimaryEntityId = targetId,
                InputParameters = inputParameter
            };

            //Execute plugin code
            fakeContext.ExecutePluginWith<Student_PreCreate>(fakePluginExecutionContext);
        }

        [TestMethod]
        public void ExeceptionScenario()
        {
            var fakeContext = new XrmFakedContext();
            //Create Target Entity
            var targetEntity = new Entity("dnlb_abc");
            var targetId = Guid.NewGuid();
            targetEntity.Id = targetId;
            var fakePluginExecutionContext = new XrmFakedPluginExecutionContext()
            {
                MessageName = "Create",
                Stage = 20,
                UserId = Guid.NewGuid(),
                PrimaryEntityName = "dnlb_student",
                PrimaryEntityId = targetId
                //InputParameters = inputParameter
            };

            Exception ex = Assert.ThrowsException<InvalidPluginExecutionException>(() => fakeContext.ExecutePluginWith<Student_PreCreate>(fakePluginExecutionContext));
        }
    }
}
