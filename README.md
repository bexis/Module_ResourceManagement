# Resource Booking Management Module
Resource booking management module: This is a calendar-based tool to manage any kind of resource. It is highly configurable to cover multiple scenarios. It helps to communicate the bookings and activities to all users (or colleagues and other stakeholders). Module for Core @BEXIS2.

1. [Features](#Features)
2. [How to use](#how_to)
    1. [Create resources](#resource_creation)
    2. [Booking resources](#booking_resources)
3. [Settings](#settings)


## Features<a name="features"></a>
- Resource with free definable properties.
- Resources have time and quantity restrictions.
- Free definable constraints on resources.
- Resources can inherit from each other.
- Resources can be tied to activities.
- Option to add notification to resources.
- Calendar view for bookings.
- Option to create events with multiple resources.
- Connection to the party package and security system of BExIS2.

## How to use <a name="how_to"></a>

### Resource creation <a name="resource_creation"></a>

1. Create a resource structure to define the properties of your resource.
2. Create a resource using your resource structure.
3. Optional: add constraints to your resource e.g. only bookable on weekends.
4. Optional: create activities that are selectable in the booking for your resources.

### Booking resources <a name="booking_resources"></a>

1. Search and select one or more resources.
2. book the resources by filling in all necessary information.

### Settings <a name="settings"></a>

- **BookingMailReceiver:** Mail receiver comma separated.
- **BookingMailReceiverCC:** Mail receiver CC comma separated.
- **BookingMailReceiverBCC:** Mail receiver BCC comma separated.
- **BookingMailSender:** No in use.
- **BookingMailSubject:** Subject of the reservation mail.
- **AlumniGroup:** Name of the alumni group. Members of this group are not displayed when booking.
- **AccountPartyTypes:** name of account party type. Is considered for the email address.
- **EventAdminGroups:** Setting admin group to edit and delete events and schedules, Groupname:resource attribute value. So you are able to give different rights to group depending on the classification with resource attribute value
- **OverNightResource:** Name of the over night resource. If you have a resource which is bookable over night then for this resource the enddate need to subtract  -1 for the validation. This is needed to make the resource bookable on the next day after the night.