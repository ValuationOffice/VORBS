administration.controller('RoomsController', ['$scope', '$http', '$resource', RoomsController]);

function RoomsController($scope, $http, $resource) {
    CreateRoomAdminServices($resource);

    $scope.Locations = Locations.query({},
        function (success) {
            $scope.currentLocation = $scope.Locations[0];
        });

    $scope.Rooms = GetAllRooms.query({});

    $scope.roomId = 0;

    $scope.SetRoomId = function (id) {
        $scope.roomId = id;
    }

    $scope.GetRoom = function (id) {
        $scope.SetRoomId(id);
        $scope.editRoom = GetRoomById.query({
            roomId: $scope.roomId
        }, function (success) {
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
        if ($scope.roomFilter.location === null) {
            $scope.Rooms = GetRoomsByFilters.query({
                status: $scope.roomFilter.status
            })
        }
        else {
            $scope.Rooms = GetRoomsByFilters.query({
                location: $scope.roomFilter.location.name,
                status: $scope.roomFilter.status
            })
        }
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
}

function CreateRoomAdminServices($resource) {
    Locations = $resource('/api/locations', {}, {
        query: { method: 'GET', isArray: true }
    });

    GetRoomById = $resource('/api/room/:roomId', { roomId: "roomId" }, {
        query: { method: 'GET' }
    });

    GetAllRooms = $resource('/api/room', {}, {
        query: { method: 'GET', isArray: true }
    });

    GetRoomsByFilters = $resource('/api/room/:location/:status', { location: "location", status: "status" }, {
        query: { method: 'GET', isArray: true }
    });

    GetRoomsByFilters.prototype = {
        smartRoomFormatted: function () {
            if (this.smartRoom) {
                return "Yes";
            }
            return "No";
        }
    };

    GetAllRooms.prototype = {
        smartRoomFormatted: function () {
            if (this.smartRoom) {
                return "Yes";
            }
            return "No";
        }
    };
}

function CreateNewRoom(newRoom) {
    $.ajax({
        type: "POST",
        data: JSON.stringify(newRoom),
        url: "api/room",
        contentType: "application/json",
        success: function (data, status) {
            alert('New room has been created.');
            ReloadRooms(null);
        },
        error: function (error) {
            alert('Unable to create new room. Please contact ITSD. ' + error.responseText);
        }
    });
}

function EditRoom(editRoom, existingId) {
    $.ajax({
        type: "POST",
        data: JSON.stringify(editRoom),
        url: "api/room" + "/" + existingId,
        contentType: "application/json",
        success: function (data, status) {
            //alert('Room updated successfully.');
            $("#room-success-alert").alert();
            $("#room-success-alert p").text('Room has been updated');
            $("#room-success-alert").fadeTo(5000, 500).slideUp(500, function () {
                $("#room-success-alert").hide();
            });
            ReloadRooms("editRoomModal");
        },
        error: function (error) {
            alert('Unable to edit room. ' + error.responseJSON.message);
        }
    });
}

function ReloadRooms(isModal) {
    if (isModal !== null) {
        $('#' + isModal).modal('hide');
    }
    var $scope = angular.element($("#roomControllerDiv")).scope();

    if ($scope.roomFilter.location === null && $scope.roomFilter.status < 0) {
        $scope.Rooms = GetAllRooms.query({});
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

    $.ajax({
        type: "POST",
        url: "api/room/" + roomId + "/" + active,
        contentType: "application/json",
        success: function (data, status) {
            //            alert('Room status has been updated.');
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
        },
        error: function (error) {
            EnableModalButton(active);
            if (active) {
                alert('Unable to enable room. Please contact ITSD. ' + error.responseJSON.message);
            }
            else {
                alert('Unable to disable room. Please contact ITSD. ' + error.responseJSON.message);
            }
        }
    });
}

function EnableModalButton(active) {
    if (active) {
        $("#enableBookingConfirmButton").prop('disabled', '');
        $("#enableBookingConfirmButton").html('Enable');
    }
    else {
        $("#disableBookingConfirmButton").prop('disabled', '');
        $("#disableBookingConfirmButton").html('Disable');
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

    /* dont need smart room validation anymore, as it is a checkbox. Can only be yes or no */
    //if ($(divNames[2] + " #roomSmart :selected").text() == "") {
    //    $(divNames[2] + " #roomSmart").addClass('has-error');
    //    errors.push('Please select if room is a SMART room.');
    //    valid = false;
    //} else {
    //    $(divNames[2] + " #roomSmart").removeClass('has-error');
    //}

    if (!valid) {
        for (var i = 0; i < errors.length; i++) {
            $(divNames[0]).append('<li>' + errors[i] + '</li>');
        }
        $(divNames[1]).css('display', 'block');
    }
    return valid;
}