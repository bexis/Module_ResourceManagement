using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace BExIS.Web.Shell.Areas.RBM.Helpers
{
    public class SendNotificationHelper
    {
        public enum BookingAction
        {
            created,
            edited,
            deleted
        }


        public static void SendNotification(List<string> receiver, string sender, string message, string subject)
        {
            List<string> receiverCC = new List<string>();
            receiverCC.Add("eleonora.petzold@uni-jena.de");

            List<string> receiverBCC = new List<string>();
            receiverBCC.Add("eleonora.petzold@uni-jena.de");

            List<string> replyToList = new List<string>();
            replyToList.Add("eleonora.petzold@uni-jena.de");

            var emailService = new EmailService();
            emailService.Send(subject, message, receiver, receiverCC, receiverBCC, replyToList);
        }

        /// <summary>
        /// Send a booking notification (create, edit or delete event) as email to in config file defined receivers
        /// </summary>
        /// <param name="bookingAction">For which action the notification will be send. (create, edit or delete event)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static void SendBookingNotification(BookingAction bookingAction, BookingEventModel model)
        {
            //string a = @"\BExIS.Modules.RBM.UI";
            //Configuration configFile = WebConfigurationManager.OpenWebConfiguration(a);
            //var map = new ExeConfigurationFileMap { ExeConfigFilename = "../RBM/web.config" };
            //var configFile = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            //get receiver and sender from the web.config file
            //List<string> receiver  = configFile.AppSettings.Settings["BookingMailReceiver"].Value.Split(',').ToList();
            //List<string> receiverCC = configFile.AppSettings.Settings["BookingMailReceiverCC"].Value.Split(',').ToList();
            //List<string> receiverBCC = configFile.AppSettings.Settings["BookingMailReceiverBCC"].Value.Split(',').ToList();
            //string sender = ConfigurationManager.AppSettings["BookingMailReceiver"];

            List<string> receiver = new List<string>();
            receiver.Add("eleonora.petzold@uni-jena.de");

            List<string> receiverCC = new List<string>();
            receiverCC.Add("eleonora.petzold@uni-jena.de");

            List<string> receiverBCC = new List<string>();
            receiverBCC.Add("eleonora.petzold@uni-jena.de");

            List<string> replyToList = new List<string>();
            replyToList.Add("eleonora.petzold@uni-jena.de");

            string subject = "Biodiversity Exploratories: Fieldbook" + bookingAction.ToString() + " reservation";

            //get mail subject from web config
            //string subject = configFile.AppSettings.Settings["BookingMailSubject"].Value;

            string message = "";
            message += "The following event has been " + bookingAction + "\n";
            message += "----------------------------------------------------------\n";
            message += "Event name: " + model.Name + "\n";
            if (model.Description != "")
                message += "Event description: " + model.Description + "\n";
            message += "Booked Resources: \n";
            message += "----------------------------------------------------------\n";

            foreach (ScheduleEventModel schedule in model.Schedules)
            {
                
                message += "Resource: " + schedule.ResourceName + "\n";
                message += "Startdate: " + schedule.ScheduleDurationModel.StartDate + "\n";
                message += "Enddate: " + schedule.ScheduleDurationModel.EndDate + "\n";
                message += "Reserved by:" + schedule.ByPerson + "\n";
                message += "Reserved for: \n";
                foreach (PersonInSchedule person in schedule.ForPersons)
                {
                    message += person.UserFullName + " \n";
                }

                message += "Contact person: " + schedule.ContactName + "( " + schedule.Contact.MobileNumber + ") \n";
                message += "---------------------------------------------------------- \n\n";
            }

            var emailService = new EmailService();
            emailService.Send(subject, message, receiver, receiverCC, receiverBCC, replyToList);
        }

    }
}