(function () {

    angular.module('vorbs.admin')
        .controller('MyBookingsController', MyBookingsController);

    MyBookingsController.$inject = ['$scope', '$resource', 'BookingsService', 'AvailabilityService', 'LocationsService'];

    function MyBookingsController($scope, $resource, BookingsService, AvailabilityService, LocationsService) {

        CreateBookingServices($resource);

        $scope.locations = LocationsService.getByStatus({ status: true, extraInfo: false })
            .$promise.then(function (resp) {
                $scope.locations = resp;
            });

        $scope.owners = Owner.getAll({});

        $scope.bookingId = 0;

        $scope.currentBookingToModify = null;

        $scope.SetBookingId = function (id) {
            $scope.bookingId = id;

            BookingsService.getByID({
                bookingId: id
            }).$promise.then(function (response) {
                $scope.currentBookingToModify = response;
            }, function (error) {
                //TODO: Error Handler
                $scope.currentBookingToModify = null;
            });
        }

        $scope.AddExternalAttendee = function () {
            SetModalErrorMessage('');
            $scope.booking.externalAttendees = AddExternalName($scope.booking.externalAttendees);
        }

        $scope.RemoveExternalAttendee = function (fullName) {
            SetModalErrorMessage('');
            $scope.booking.externalAttendees = RemoveExternalName(fullName, $scope.booking.externalAttendees);
        }

        $scope.externalFullNameTextBox = '';
        $scope.externalCompanyNameTextBox = '';
        $scope.externalPassRequired = false;

        $('#fullNameTextBox').typeahead({
            hint: false,
            highlight: true,
            minLength: 3
        },
            {
                name: 'owners',
                source: SubstringMatcher($scope.owners)
            });

        $scope.FormatPassRequired = function (required) {
            if (required) {
                return "Yes";
            }
            else {
                return "No";
            }
        }

        $scope.LoadEditBooking = function (bookingId) {
            //Reset Any Error Messages
            SetModalErrorMessage('');
            ResetExternalNamesUI();
            SetEditActiveTab('editBooking');

            $scope.bookingId = bookingId;

            BookingsService.getByID({
                bookingId: bookingId
            }).$promise.then(function (response) {

                $scope.newBooking.Room.RoomName = response.room.roomName;
                $scope.newBooking.Subject = response.subject;
                $scope.newBooking.FlipChart = response.flipchart;
                $scope.newBooking.Projector = response.projector;
                $scope.newBooking.RoomID = response.room.id;

                if (response.externalAttendees !== null) {
                    $scope.booking.externalAttendees = response.externalAttendees;
                }
                else {
                    $scope.booking.externalAttendees = []; //Reset External Name List
                }

                $scope.booking.numberOfAttendees = response.numberOfAttendees;
                $scope.booking.startTime = FormatTimeDate(response.startDate, false);
                $scope.booking.endTime = FormatTimeDate(response.endDate, false);
                $scope.booking.date = FormatTimeDate(response.startDate, true);

                //Store the vital existing booking data
                $scope.existingBooking.numberOfAttendees = $scope.booking.numberOfAttendees;
                $scope.existingBooking.startTime = $scope.booking.startTime;
                $scope.existingBooking.endTime = $scope.booking.endTime;
                $scope.existingBooking.date = $scope.booking.date;

                $("#editModal #bookingDate").datepicker('update');

                $scope.editBooking = response;

            }, function (error) {
                $scope.editBooking = null;
            });
        }

        $scope.CheckEditBooking = function () {
            //Reset Any Error Messages
            SetModalErrorMessage('');

            //Validate Start Date
            if ($scope.booking.date === "") {
                SetModalErrorMessage('Invalid date.');
                return;
            }

            //Validate Number of External Names is not graether than attendees
            ValidateNoAttendees($scope.booking.numberOfAttendees, $scope.booking.externalAttendees.length);
            $scope.newBooking.NumberOfAttendees = $scope.booking.numberOfAttendees;

            var unsavedAttendee = ValidateUnSavedAttendee();
            if (unsavedAttendee !== "") {
                SetModalErrorMessage(unsavedAttendee);
                return;
            }

            //Validate Times
            var timeValidation = ValidateStartEndTime($scope.booking.startTime, $scope.booking.endTime);
            if (timeValidation !== "") {
                SetModalErrorMessage(timeValidation);
                return;
            }

            //Change the "accept booking" button to stop multiple bookings
            $("#acceptBookingConfirmButton").prop('disabled', 'disabled');
            $("#acceptBookingConfirmButton").html('Editing Booking. Please wait..');

            try {
                //Create Date String
                $scope.newBooking.StartDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true, true);
                $scope.newBooking.EndDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true, true);

                if ($scope.booking.externalAttendees.length > 0) {
                    //$scope.newBooking.externalAttendees = $scope.booking.ExternalNames.join(';');
                    $scope.newBooking.externalAttendees = $scope.booking.externalAttendees;
                }

                //Validate if Date/Time/Attendees has changed
                if (($scope.booking.numberOfAttendees === $scope.existingBooking.numberOfAttendees) && ($scope.booking.date === $scope.existingBooking.date) &&
                    ($scope.booking.startTime === $scope.existingBooking.startTime) && ($scope.booking.endTime === $scope.existingBooking.endTime)) {
                    saveBooking($scope.bookingId, $scope.newBooking);
                }
                else {
                    $scope.availableRooms = AvailabilityService.get({
                        location: $scope.editBooking.location.name,
                        start: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true, true),
                        smartRoom: false,
                        end: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true, true),
                        numberOfPeople: $scope.booking.numberOfAttendees,
                        existingBookingId: $scope.bookingId
                    }).$promise.then(function (success) {

                        $scope.availableRooms = success;
                        if ($scope.availableRooms.length === 0) {
                            EnableAcceptBookingButton();
                            SetModalErrorMessage('No rooms avaiLable using the below Date/Time/Attendees.');
                        }
                        else if ($scope.availableRooms[0].roomName === $scope.newBooking.Room.RoomName) {
                            saveBooking($scope.bookingId, $scope.newBooking);
                        }
                        else {
                            EnableAcceptBookingButton();
                            SetEditActiveTab('confirmEditBooking');
                            $scope.currentRoom = $scope.availableRooms[0];
                        }
                    },
                        function (error) {
                            EnableAcceptBookingButton();
                            alert('Unable to Edit. Please Try Again or Contact ITSD.');
                        });
                }
            } catch (e) {
                EnableAcceptBookingButton();
            }
        }

        $scope.EditBooking = function () {
            //Change the "accept booking" button to stop multiple bookings
            $("#confirmBookingConfirmButton").prop('disabled', 'disabled');
            $("#confirmBookingConfirmButton").html('Editing Booking. Please wait..');

            $scope.newBooking.RoomID = $scope.currentRoom.id;
            saveBooking($scope.bookingId, $scope.newBooking);

            //Disabled as the page refresh after save render the page with button enabled. By activating it here, there is a brief period where the user can select
            //the button twice, and give a false positive for a secondary failure of the same save
            //EnableConfirmBookingButton();
        }

        function saveBooking(id, booking) {
            BookingsService.update({
                existingId: id,
                recurrence: booking.recurrence || false
            }, booking).$promise.then(function (response) {

                alert('Booking Updated Successfully.');
                ReloadThisPage("bookings");

            }, function (error) {
                //Server Conflict
                if (error.status == 406) {
                    alert("Simultaneous booking conflict. Please try again.");
                    ReloadThisPage("bookings");
                }
                else {
                    alert('Unable to edit meeting room. Please contact ITSD.');
                }
                EnableEditBookingButton();
            });
        }

        $scope.modifyBookingOptions = resetModifyBookingOptions();

        function resetModifyBookingOptions() {
            return {
                deleteAllInRecurrence: false
            };
        }

        $scope.DeleteBooking = function () {
            //Change the "delete booking" button to stop multiple bookings
            $("#deleteBookingConfirmButton").prop('disabled', 'disabled');
            $("#deleteBookingConfirmButton").html('Deleting booking. Please wait..');

            try {
                BookingsService.remove(
                    {
                        bookingId: $scope.bookingId,
                        recurrence: $scope.modifyBookingOptions.deleteAllInRecurrence
                    }).$promise.then(function (success) {
                        $('#deleteModal').modal('hide');
                        $scope.SearchBooking();
                        EnableDeleteBookingButton();
                    }, function (error) {
                        alert('Unable to delete the booking. Please try again or contact ITSD.');
                        EnableDeleteBookingButton();
                    });
            } catch (e) {
                EnableDeleteBookingButton();
            }
        }

        $scope.SearchBooking = function () {
            SetAdminBookingErrorMessage('');

            //Validate input fields
            if ($('#fullNameTextBox').typeahead('val').trim() <= 0 && $scope.bookingFilter.room == "" && $scope.bookingFilter.location == "" && $scope.bookingFilter.startDate == "") {
                SetAdminBookingErrorMessage("Select at least one value");
                return;
            }

            BookingsService.search({
                location: $scope.bookingFilter.location.id,
                room: $scope.bookingFilter.room,
                owner: $('#fullNameTextBox').typeahead('val'),
                start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + "12:00", 'MM-DD-YYYY', true, true)
            }).$promise.then(function (success) {
                if (success.length === 0) {
                    SetAdminBookingErrorMessage('No bookings found.');
                }

                $scope.bookings = success;
            });
        }

        $scope.newBooking = {
            Room: { RoomName: '' },
            Subject: '',
            NumberOfAttendees: 0,
            RoomID: 0,
            externalAttendees: [],
            StartDate: new Date(),
            EndDate: new Date(),
            FlipChart: false,
            Projector: false
        };

        $scope.booking = {
            startTime: '',
            endTime: '',
            date: new Date(),
            numberOfAttendees: 0,
            externalAttendees: []
        }

        $scope.existingBooking = {
            startTime: '',
            endTime: '',
            date: new Date(),
            numberOfAttendees: 0
        }

        $scope.bookingFilter = {
            location: '',
            room: '',
            fullName: '',
            startDate: new moment().utc().format('DD-MM-YYYY')
        }
    }

    function CreateBookingServices($resource) {
        Owner = $resource('/api/admin', {}, {
            getAll: { method: 'GET', isArray: true }
        });
    }

    function SetAdminBookingErrorMessage(message) {
        if (message === "") {
            $('#adminBookingErrorMessage').hide();
        }
        else {
            $('#adminBookingErrorMessage').text(message);
            $('#adminBookingErrorMessage').show();
        }
    }

    function SubstringMatcher(strs) {
        return function findMatches(q, cb) {
            var matches, substringRegex;

            // an array that will be populated with substring matches
            matches = [];

            // regex used to determine if a string contains the substring `q`
            substrRegex = new RegExp(q, 'i');

            // iterate through the pool of strings and for any string that
            // contains the substring `q`, add it to the `matches` array
            $.each(strs, function (i, str) {
                if (substrRegex.test(str)) {
                    matches.push(str);
                }
            });

            cb(matches);
        };
    };

})();