newBooking2.controller('NewBookingController2', ['$scope', '$http', '$resource', NewBookingController2]);

function NewBookingController2($scope, $http, $resource) {
    CreateServices($resource);

    InitiateDateTimeCalanders();

    $scope.locations = Locations.query({});
    $scope.currentLocation = $scope.locations[0];

    $scope.SearchBookings = function () {
        $scope.roomBookings = Available.query({
            location: $scope.bookingFilter.location.name,
            startDate: FormatDate($scope.bookingFilter.startDate),
            endDate: FormatDate($scope.bookingFilter.startDate)
        },
        function (success) {
            var rooms = [];
            var eventData = [];
            var overLapping;

            var parms = [ValidateAttendess($scope.bookingFilter.numberOfAttendees), FormatTime($scope.bookingFilter.startTime, $scope.bookingFilter.startDate), FormatTime($scope.bookingFilter.endTime, $scope.bookingFilter.startDate)]

            ValidateDates(parms[1], parms[2]);

            for (var i = 0; i < success.length; i++) {
                overLapping = false;

                if (success[i].bookings.length === 0 && success[i].seatCount >= parms[0]) {
                    eventData.push({ room: success[i].roomName });
                    continue;
                }

                for (var k = 0; k < success[i].bookings.length; k++) {

                    //Filter the results
                    if (success[i].seatCount >= parms[0]) {

                        if (parms[2] > ExtractTimeFromDate(success[i].bookings[k].startDate) && parms[1] < ExtractTimeFromDate(success[i].bookings[k].endDate)) {
                            overLapping = true;
                        }

                        if (overLapping) {
                            break;
                        }
                        else if (success[i].bookings.length !== (k + 1)) {
                            continue;
                        }

                        eventData.push(
                        {
                            room: success[i].roomName
                            //start: success[i].bookings[k].startDate,
                            //end: success[i].bookings[k].endDate
                        });
                        break;
                    }
                }
            }

            $('#bookingTableBody').html('');
            $('#errorMessage').html('');

            for (var i = 0; i < eventData.length; i++) {
                $('#bookingTableBody').append('<tr><td>' + eventData[i].room + '</td><td>' + parms[1].toDateString() + " " + parms[1].toLocaleTimeString() +
                    '</td><td>' + parms[2].toDateString() + " " + parms[2].toLocaleTimeString() +
                    '</td><td><span data-toggle="modal" data-target="#newModal" class="glyphicon glyphicon-ok btn"></span></td></tr>');
            }

            if (eventData.length == 0) {

                $('#errorMessage').append('<h4 style="color:red">No Rooms Available. Here are Some Suggested Rooms.</h4>')

                //Hardcoded suggested rooms
                $('#bookingTableBody').append('<tr><td>MR8</td><td>Wed May 13 2015  ‎<b>‎10‎:00‎:‎00</b></td><td>Wed May 13 2015 <b>‎11‎:‎00:‎00</b></td><td><span class="glyphicon glyphicon-ok btn"></span></td></tr>');
                $('#bookingTableBody').append('<tr><td>MR21</td><td>Wed May 13 2015 ‎<b>10:‎00‎:‎00</b></td><td>Wed May 13 2015 ‎<b>11‎:00‎:‎00</b></td><td><span class="glyphicon glyphicon-ok btn"></span></td></tr>');
                $('#bookingTableBody').append('<tr><td>MR16</td><td><b>Thur May 14 2015</b> ‎10:‎30‎:‎00</td><td><b>Thur May 14 2015</b> ‎11‎:30‎:‎00</td><td><span class="glyphicon glyphicon-ok btn"></span></td></tr>');
                $('#bookingTableBody').append('<tr><td>MR18</td><td><b>Thur May 14 2015</b> ‎10:‎30‎:‎00</td><td><b>Thur May 14 2015</b> ‎11‎:30‎:‎00</td><td><span class="glyphicon glyphicon-ok btn"></span></td></tr>');
            }
        },
            function (error) {
                alert('Error');
            }
        );
    }


    $scope.bookingFilter = {
        startDate: '14-05-2015',
        startTime: $('.timepicker').val(),
        endTime: $('.timepicker').val(),
        location: $scope.currentLocation,
        smartRoom: false,
        numberOfAttendees: 1
    };

    $scope.booking = {
        StartDate: new Date(),
        EndDate: new Date(),
        Room: { RoomName: '' },
        Location: { LocationName: '' }
    };

    $scope.newBooking = {
        roommName: { RoomName: '' },
        internalAttendes: null,
        externalAttendes: null,
        pc: false,
        flipChart: false,
        projector: false,
    };
}

function InitiateDateTimeCalanders() {
    $('.datepicker').datepicker({
        format: 'dd-mm-yyyy',
        autoClose: true,
        todayBtn: true,
        todayHighlight: true,
        weekStart: 1
    })

    $('.timepicker').timepicker({
        showInputs: false,
        minuteStep: 30
    });

    //$('#endTimePicker').timepicker({
    //    defaultTime: IncrementTime($('#endTimePicker').val(), 30)
    //})
}

function CreateServices($resource) {
    Locations = $resource('/api/locations', {
    }, {
        query: { method: 'GET', isArray: true }
    })

    Available = $resource('/api/availability/:location/:startDate/:endDate', { location: 'location', startDate: 'startDate', endDate: 'endDate' },
    {
        query: { method: 'GET', isArray: true }
    });
}

function ExtractTimeFromDate(date) {
    var dateParts = date.split('T');

    var dateSplit = dateParts[0].split('-');
    var timeSplit = dateParts[1].split(':');

    var timeDate = new Date(dateSplit[0], dateSplit[1] - 1, dateSplit[2]);
    timeDate.setHours(timeSplit[0], timeSplit[1], timeSplit[2]);

    return timeDate;
}

function FormatDate(date) {

    if (date === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    var parts = date.split('-');
    myDate2 = new Date(parts[2], parts[1] - 1, parts[0]);

    return moment(myDate2).format('MM-DD-YYYY');
}

function FormatTime(time, date) {
    if (time === "") {
        alert('Invalid Time');
        throw new Error();
    }

    var parts = date.split('-');
    var timeSplit = convertTo24Hour(time).split(':');

    if (timeSplit[0] > 17 || (timeSplit[0] == 17 && timeSplit[1] > 0)) {
        alert('Time Must Be Before 5.00 PM');
        throw new Error();
    }
    else if (timeSplit[0] < 9) {
        alert('Time Must Be After 9.00 AM');
        throw new Error();
    };


    var timeDate = new Date(parts[2], parts[1] - 1, parts[0]);
    timeDate.setHours(timeSplit[0], timeSplit[1], 0);

    return timeDate;
}

function ValidateAttendess(attendees) {

    if (attendees === null) {
        throw new Error();
    }
    else if (isNaN(attendees)) {
        alert('Invalid Number');
        throw new Error();
    }
    else if (attendees <= 0) {
        alert('Attendees Can Not Be Negative');
        throw new Error();
    }
    return attendees;
}

function ValidateDates(start, end) {
    //ToDo: Add Equals then
    if (start > end) {
        alert('Start Date Can Not Be Ahead Of End Date');
        throw new Error();
    }
}

function convertTo24Hour(time) {
    var hours = parseInt(time.substr(0, 2));
    time = time.toString().trim();
    if (time.indexOf('AM') != -1 && hours == 12) {
        time = time.replace('12', '0');
    }
    if (time.indexOf('PM') != -1 && hours < 12) {
        time = time.replace(hours, (hours + 12));
    }
    return time.replace(/(AM|PM)/, '').slice(0, -1);
}

function IncrementTime(time, minutes) {
    var timeSplit = time.split(':');
    var amPm = timeSplit[1].split(" ");

    minutes = parseInt(amPm[0]) + minutes

    if (minutes >= 60) {
        minutes -= 60;
        ("0" + minutes).slice(-2);
    }

    return "'" + timeSplit[0] + ":" + minutes + " " + amPm[1] + "'";
}
