using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using Vaiona.Utils.Cfg;

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
            //get receiver and sender from the settings file
            var receiver = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiver").ToString().Split(',').ToList();
            var receiverCC = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiverCC").ToString().Split(',').ToList();
            var receiverBCC = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiverBCC").ToString().Split(',').ToList();

            var sender = Modules.RBM.UI.Helper.Settings.get("BookingMailSender").ToString();

            var subject = Modules.RBM.UI.Helper.Settings.get("BookingMailSubject").ToString();

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
            emailService.Send(subject, message, receiver, receiverCC, receiverBCC);
        }

    }
}