(function () {

    angular.module('vorbs.admin')
        .controller('RoomsController', RoomsController);

    RoomsController.$inject = ['$scope', 'LocationsService', 'RoomsService'];

    function RoomsController($scope, LocationsService, RoomsService) {

        $scope.Locations = LocationsService.query().$promise.then(
            function (resp) {
                $scope.Locations = resp;
                $scope.currentLocation = $scope.Locations[0];
            });

        $scope.Rooms = RoomsService.query().$promise.then(function (resp) {
            $scope.Rooms = resp;
        });

        $scope.roomId = 0;

        $scope.SetRoomId = function (id) {
            $scope.roomId = id;
        }

        $scope.GetRoom = function (id) {
            $scope.SetRoomId(id);
            $scope.editRoom = RoomsService.getByID({
                id: $scope.roomId
            }).$promise.then(function (resp) {
                $scope.editRoom = resp;
                var roomName = $scope.editRoom.roomName;
                $scope.currentEditRoom = roomName;
            });
        }

        $scope.EditRoom = function () {
            var isValid = ValidateRoom(true, $scope.Rooms, $scope.editRoom.roomName, $scope.editRoom.location.name);

            if (isValid) {
                EditRoom($scope.editRoom, $scope.roomId);
            }
        }

        $scope.NewRoom = function () {
            var isValid = ValidateRoom(false, $scope.Rooms, $scope.newRoom.RoomName, $scope.currentLocation);

            if (isValid) {
                $scope.newRoom.LocationID = $scope.currentLocation.id;
                CreateNewRoom($scope.newRoom);
            }
        }

        $scope.DisableRoom = function () {
            EnableDisableRoom($scope.roomId, false);
        }

        $scope.EnableRoom = function () {
            EnableDisableRoom($scope.roomId, true);
        }

        $scope.GetRoomsByFilter = function () {

            var location = $scope.roomFilter.location === null ? "location" : $scope.roomFilter.location.name;

            $scope.Rooms = RoomsService.getByStatus({
                status: $scope.roomFilter.status,
                locationName: location
            }).$promise.then(function (resp) {
                $scope.Rooms = resp;
            });
        }

        $scope.roomFilter = {
            location: null,
            status: -1,
        }

        $scope.newRoom = {
            Active: false,
            RoomName: '',
            LocationID: 0,
            ComputerCount: 0,
            PhoneCount: 0,
            SeatCount: 1,
            SmartRoom: false,
        }

        function CreateNewRoom(newRoom) {

            RoomsService.create({}, newRoom).$promise.then(function () {
                alert('New room has been created.');
                ReloadRooms(null);
            }, function () {
                alert('Unable to create new room. Please contact ITSD.');
            });
        }

        function EditRoom(editRoom, existingId) {

            RoomsService.update({ id: existingId }, editRoom).$promise.then(function () {
                $("#room-success-alert").alert();
                $("#room-success-alert p").text('Room has been updated');
                $("#room-success-alert").fadeTo(5000, 500).slideUp(500, function () {
                    $("#room-success-alert").hide();
                });
                ReloadRooms("editRoomModal");
            }, function () {
                alert('Unable to edit room.');
            });
        }

        function ReloadRooms(isModal) {
            if (isModal !== null) {
                $('#' + isModal).modal('hide');
            }
            var $scope = angular.element($("#roomControllerDiv")).scope();

            if ($scope.roomFilter.location === null && $scope.roomFilter.status < 0) {
                $scope.Rooms = RoomsService.query().$promise.then(function (resp) {
                    $scope.Rooms = resp;
                });
            }
            else {
                $scope.GetRoomsByFilter();
            }
        }

        function EnableDisableRoom(roomId, active) {

            if (active) {
                $("#enableBookingConfirmButton").prop('disabled', 'disabled');
                $("#enableBookingConfirmButton").html('Enabling Room. Please wait..');
            } else {
                $("#disableBookingConfirmButton").prop('disabled', 'disabled');
                $("#disableBookingConfirmButton").html('Disabling Room. Please wait..');
            }

            RoomsService.updateStatus({
                id: roomId,
                status: active
            }).$promise.then(function () {
                $("#room-success-alert").alert();
                $("#room-success-alert p").text('Room status has been updated');
                $("#room-success-alert").fadeTo(5000, 500).slideUp(500, function () {
                    $("#room-success-alert").hide();
                });
                if (active) {
                    ReloadRooms("enableRoomModal");
                }
                else {
                    ReloadRooms("disableRoomModal");
                }
                EnableModalButton(active);
            }, function () {
                EnableModalButton(active);
                if (active) {
                    alert('Unable to enable room. Please contact ITSD.');
                }
                else {
                    alert('Unable to disable room. Please contact ITSD.');
                }
            });
        }

        function EnableModalButton(active) {
            if (active) {
                $("#enableBookingConfirmButton").prop('disabled', '');
                $("#enableBookingConfirmButton").html('Enable Room');
            }
            else {
                $("#disableBookingConfirmButton").prop('disabled', '');
                $("#disableBookingConfirmButton").html('Disable Room');
            }
        }

        function ValidateRoom(edit, existingRooms, newRoom, newLocation) {
            var valid = true;
            var divNames;

            if (edit) {
                divNames = ["#editRoomErrorList", "#editRoomErrorCont", "#editRoom"];
            }
            else {
                divNames = ["#addRoomErrorList", "#addRoomErrorCont", "#newRoom"];
            }

            $(divNames[0]).html('');
            $(divNames[1]).css('display', 'none');
            var errors = [];

            if ($(divNames[2] + " #roomName input").val().trim() === "") {
                $(divNames[2] + " #roomName").addClass('has-error');
                errors.push('Please enter a valid room name.');
                valid = false;
            } else {
                var duplicateRoom = false;

                //TODO: Need to change this so it allows the same room name if editing
                if (!edit) {
                    //Check to see if room name already exists at location
                    for (var i = 0; i < existingRooms.length; i++) {
                        if (existingRooms[i].roomName.toUpperCase() === newRoom.toUpperCase() && existingRooms[i].location.name === newLocation.name) {
                            duplicateRoom = true;
                        }
                    }
                }

                if (duplicateRoom) {
                    $(divNames[2] + " #roomName").addClass('has-error');
                    errors.push('Room already exists at selected location.');
                    valid = false;
                }
                else {
                    $(divNames[2] + " #roomName").removeClass('has-error');
                }
            }

            if ($(divNames[2] + " #roomComputer input").val().trim() === "") {
                $(divNames[2] + " #roomComputer").addClass('has-error');
                errors.push('Please enter number of computers.');
                valid = false;
            } else {
                $(divNames[2] + " #roomComputer").removeClass('has-error');
            }

            if ($(divNames[2] + " #roomPhone input").val().trim() === "") {
                $(divNames[2] + " #roomPhone").addClass('has-error');
                errors.push('Please enter number of telephones.');
                valid = false;
            } else {
                $(divNames[2] + " #roomPhone").removeClass('has-error');
            }

            if ($(divNames[2] + " #roomSeatCount input").val().trim() === "") {
                $(divNames[2] + " #roomSeatCount").addClass('has-error');
                errors.push('Please enter number of seats.');
                valid = false;
            } else {
                $(divNames[2] + " #roomSeatCount").removeClass('has-error');
            }

            if (!valid) {
                for (var i = 0; i < errors.length; i++) {
                    $(divNames[0]).append('<li>' + errors[i] + '</li>');
                }
                $(divNames[1]).css('display', 'block');
            }
            return valid;
        }
    }

})();