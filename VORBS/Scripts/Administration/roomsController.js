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

    $scope.NewRoom = function () {
        var isValid = ValidateNewRoom($scope.Rooms, $scope.newRoom.RoomName, $scope.currentLocation);

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
            location.reload();
        },
        error: function (error) {
            alert('Unable to create new room. Please contact ITSD. ' + error.responseJSON.message);
        }
    });
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
                alert('Room status has been updated.');
                location.reload();
            },
            error: function (error) {
                if (active) {
                    alert('Unable to enable room. Please contact ITSD. ' + error.responseJSON.message);
                    $("#enableBookingConfirmButton").prop('disabled', '');
                    $("#enableBookingConfirmButton").html('Enable');
                }
                else {
                    alert('Unable to disable room. Please contact ITSD. ' + error.responseJSON.message);
                    $("#disableBookingConfirmButton").prop('disabled', '');
                    $("#disableBookingConfirmButton").html('Disable');
                }
            }
        });
  
}

function ValidateNewRoom(existingRooms, newRoom, newLocation) {
    var valid = true;

    $("#addRoomErrorList").html('');
    $("#addRoomErrorCont").css('display', 'none');
    var errors = [];

    if ($("#newRoom #newRoomName input").val().trim() === "") {
        $("#newRoom #newRoomName").addClass('has-error');
        errors.push('Please enter a valid room name.');
        valid = false;
    } else {
        var duplicateRoom = false;
        //Check to see if room name already exists at location
        for (var i = 0; i < existingRooms.length; i++) {
            if (existingRooms[i].roomName.toUpperCase() === newRoom.toUpperCase() && existingRooms[i].location.name === newLocation.name) {
                duplicateRoom = true;
            }
        }
        if (duplicateRoom) {
            $("#newRoom #newRoomName").addClass('has-error');
            errors.push('Room already exists at selected location.');
            valid = false;
        }
        else {
            $("#newRoom #newRoomName").removeClass('has-error');
        }
    }

    if ($("#newRoom #newRoomComputer input").val().trim() === "") {
        $("#newRoom #newRoomComputer").addClass('has-error');
        errors.push('Please enter number of computers.');
        valid = false;
    } else {
        $("#newRoom #newRoomComputer").removeClass('has-error');
    }

    if ($("#newRoom #newRoomPhone input").val().trim() === "") {
        $("#newRoom #newRoomPhone").addClass('has-error');
        errors.push('Please enter number of telephones.');
        valid = false;
    } else {
        $("#newRoom #newRoomPhone").removeClass('has-error');
    }


    if ($("#newRoom #newRoomSeatCount input").val().trim() === "") {
        $("#newRoom #newRoomSeatCount").addClass('has-error');
        errors.push('Please enter number of seats.');
        valid = false;
    } else {
        $("#newRoom #newRoomSeatCount").removeClass('has-error');
    }

    if ($("#newRoom #newRoomSmart select")[0].selectedIndex < 0) {
        $("#newRoom #newRoomSmart").addClass('has-error');
        errors.push('Please select if room is a SMART room.');
        valid = false;
    } else {
        $("#newRoom #newRoomSmart").removeClass('has-error');
    }

    if (!valid) {
        for (var i = 0; i < errors.length; i++) {
            $("#addRoomErrorList").append('<li>' + errors[i] + '</li>');
        }
        $("#addRoomErrorCont").css('display', 'block');
    }
    return valid;
}