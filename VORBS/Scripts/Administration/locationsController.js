(function () {

    angular.module('vorbs.admin')
        .controller('LocationsController', LocationsController);

    LocationsController.$inject = ['$scope', 'LocationsService'];

    function LocationsController($scope, LocationsService) {

        $scope.Locations = LocationsService.query().$promise.then(function (resp) {
            $scope.Locations = resp;
        });

        $scope.locationId = 0;

        $scope.SetLocation = function (location) {
            $scope.pickedLocation =
                {
                    ID: location.id,
                    Name: location.name,
                    Rooms: location.rooms,
                    Active: location.active
                };
        }

        $scope.NewEditLocation = {};
        $scope.OriginalEditLocation = {};

        $scope.GetLocationsByStatus = function () {
            if ($scope.selectedItem.type === "all") {
                $scope.Locations = LocationsService.query().$promise.then(function (resp) {
                    $scope.Locations = resp;
                });
            } else {
                var status = $scope.selectedItem.type === "enabled" ? true : false;

                $scope.Locations = LocationsService.getByStatus({
                    status: status,
                    extraInfo: true
                }).$promise.then(function (resp) {
                    $scope.Locations = resp;
                });
            }
        }

        $scope.locationStatuses = [
            { id: 0, name: "All Locations", value: "all" },
            { id: 1, name: "Enabled Locations", value: "enabled" },
            { id: 2, name: "Disabled Locations", value: "disabled" }
        ];

        $scope.selectedItem = {
            type: $scope.locationStatuses[0].value,
        };

        $scope.status = {
            open: true,
            disabled: false
        };

        $scope.DisableLocation = function () {
            EnableDisableLocation($scope.pickedLocation.ID, false);
        }

        $scope.EnableLocation = function () {
            EnableDisableLocation($scope.pickedLocation.ID, true);
        }

        $($('#newLocationModal').on('show.bs.modal', function () {
            SetLocationModalErrorMessage("");
        }))

        $('#newLocationModal').on('hidden.bs.modal', function () {
            $scope.newLocation.Name = '',
                $scope.newLocation.additionalInformation = '',
                $scope.newLocation.Active = false,
                $scope.newLocation.LocationCredentials = [],
                $scope.$digest()
        })


        $scope.newLocation = {
            Name: '',
            Active: false,
            LocationCredentials: [
                {
                    Department: 'facilities',
                    Phone: '',
                    Email: '',
                    Person: ''
                },
                {
                    Department: 'security',
                    Phone: '',
                    Email: '',
                    Person: ''
                },
                {
                    Department: 'dss',
                    Phone: '',
                    Email: '',
                    Person: ''
                }]
        };


        $scope.AddNewLocation = function () {
            SetLocationModalErrorMessage("");

            // Change 'Confirm Location' button to stop multiple locations
            $('#confirmLocationBtn').prop('disabled', 'disabled');
            $('#confirmLocationBtn').html('Creating Location. Please wait...');

            try {
                // Validate Location Name
                var locations = [];
                for (var i = 0; i < $scope.Locations.length; i++) {
                    locations.push($scope.Locations[i].name);
                }


                var emails = [];
                for (var i = 0; i < $scope.newLocation.LocationCredentials.length; i++) {
                    emails.push($scope.newLocation.LocationCredentials[i].Email);
                }

                var isValid = ValidateNewLocation($scope.newLocation.Name, locations, emails);

                if (isValid) {

                    LocationsService.save({}, $scope.newLocation).$promise.then(function (success) {
                        $('#newLocationModal').modal('hide');
                        var $roomScope = angular.element($("#roomControllerDiv")).scope();

                        $scope.Locations = LocationsService.query().$promise.then(function (resp) {
                            $scope.Locations = resp;
                            $roomScope.Locations = resp;
                            $roomScope.currentLocation = $roomScope.Locations[0];

                        }).finally(function () {
                            $roomScope.$digest();
                        });

                    }, function (error) {
                        alert('Unable to create new Location.');
                        $('#confirmLocationBtn').prop('disabled', '');
                        $('#confirmLocationBtn').html('Confirm Location');
                    });
                };
                $('#confirmLocationBtn').prop('disabled', '');
                $('#confirmLocationBtn').html('Confirm Location');

            } catch (e) {
                $('#confirmLocationBtn').prop('disabled', '');
                $('#confirmLocationBtn').html('Confirm Location');
            }
        }

        $scope.SetNewEditLocation = function (_id) {
            $scope.NewEditLocation = LocationsService.getByID({ id: _id }).$promise.then(function (success) {
                $scope.NewEditLocation = success;

                var newLoc = success;
                $scope.OriginalEditLocation = { name: success.name, id: success.id };
                $scope.NewEditLocation.GetLocationCredentials = function (department) {
                    for (var i = 0; i < this.locationCredentials.length; i++) {
                        if (this.locationCredentials[i].department == department) {
                            return this.locationCredentials[i];
                        }
                    }
                }
            });
        }

        $scope.EditLocation = function () {
            $("#editLocationConfirm").prop('disabled', 'disabled');
            $("#editLocationConfirm").html('Enabling Location. Please wait..');
            if (ValidateEditLocation($scope.NewEditLocation, $scope.OriginalEditLocation, $scope.Locations)) {

                var credentials = [];

                var facilityDetail = { department: 'facilities', email: $("#facilitiesEmail").val().trim(), phoneNumber: $("#facilitiesPhone").val().trim() };
                if (facilityDetail.email != "" || facilityDetail.phoneNumber != "") {
                    credentials.push(facilityDetail);
                }

                var securityDetail = { department: 'security', email: $("#securityEmail").val().trim(), phoneNumber: $("#securityPhone").val().trim() };
                if (securityDetail.email != "" || securityDetail.phoneNumber != "") {
                    credentials.push(securityDetail);
                }

                var dssDetail = { department: 'dss', email: $("#dssEmail").val().trim(), phoneNumber: $("#dssPhone").val().trim() };
                if (dssDetail.email != "" || dssDetail.phoneNumber != "") {
                    credentials.push(dssDetail);
                }

                $scope.NewEditLocation.locationCredentials = credentials;

                LocationsService.update({ id: $scope.OriginalEditLocation.id }, $scope.NewEditLocation).$promise.then(function (success) {
                    $scope.GetLocationsByStatus();

                    $("#editLocationConfirm").prop('disabled', '');
                    $("#editLocationConfirm").html('Accept Changes');
                    $("#editLocationModal").modal('hide');

                    $("#location-success-alert").alert();
                    $("#location-success-alert p").text('Location has been updated');
                    $("#location-success-alert").fadeTo(5000, 500).slideUp(500, function () {
                        $("#location-success-alert").hide();
                    });

                }, function (error) {
                    $("#editLocationConfirm").prop('disabled', '');
                    $("#editLocationConfirm").html('Accept Changes');
                    ClearEditLocationErrors();
                    AddEditLocationErrorMessage('Error occured when editting the location. Please try again or contact ITSD');
                });
            } else {
                $("#editLocationConfirm").prop('disabled', '');
                $("#editLocationConfirm").html('Accept Changes');
            }

        }

        function EnableDisableLocation(locationid, active) {
            var $scope = angular.element($("#locationsControllerDiv")).scope();
            if (active) {
                $("#enableLocationConfirmButton").prop('disabled', 'disabled');
                $("#enableLocationConfirmButton").html('Enabling Location. Please wait..');
            } else {
                $("#disableLocationConfirmButton").prop('disabled', 'disabled');
                $("#disableLocationConfirmButton").html('Disabling Location. Please wait..');
            }

            LocationsService.updateStatus({
                id: locationid,
                status: active
            }).$promise.then(function () {

                if (active) {
                    $('#enableLocationModal').modal('hide');
                    $("#enableLocationConfirmButton").prop('disabled', '');
                    $("#enableLocationConfirmButton").html('Enable Location');
                }
                else {
                    $('#disableLocationModal').modal('hide');
                    $("#disableLocationConfirmButton").prop('disabled', '');
                    $("#disableLocationConfirmButton").html('Disable Location');
                }
                $scope.GetLocationsByStatus();
                $("#location-success-alert").alert();
                $("#location-success-alert p").text('Location Status has been updated.');
                $("#location-success-alert").fadeTo(5000, 500).slideUp(500, function () {
                    $("#location-success-alert").hide();
                });

            }, function (error) {

                if (active) {
                    alert('Unable to enable location. Please contact ITSD.');
                    $("#enableLocationConfirmButton").prop('disabled', '');
                    $("#enableLocationConfirmButton").html('Enable Location');
                }
                else {
                    alert('Unable to disable location. Please contact ITSD.');
                    $("#disableLocationConfirmButton").prop('disabled', '');
                    $("#disableLocationConfirmButton").html('Disable Location');
                }
            });
        }

    }

    function ValidateNewLocation(newLocationName, locations, emails) {
        ClearEditLocationErrors();
        var valid = true;

        // Validate Location Name
        if (newLocationName === undefined || newLocationName === '') {
            SetLocationModalErrorMessage("Invalid location name");
            valid = false;
        }
        else {
            // Check if Location Name already exists
            for (var i = 0; i < locations.length; i++) {
                if (newLocationName.toLowerCase() === locations[i].toLowerCase()) {
                    SetLocationModalErrorMessage("Location name already exists");
                    valid = false;
                }
            }
        }

        //Validate Each Email
        for (var i = 0; i < emails.length; i++) {
            if (emails[i] === "") {
                continue;
            }
            if (!ValidateEmail(emails[i])) {
                SetLocationModalErrorMessage("Invalid email detected: " + emails[i]);
                valid = false;
            }
        }

        return valid;
    }

    //Modal Functions
    function SetLocationModalErrorMessage(message) {
        if (message === "") {
            $('#LocationModalErrorMessage').hide();
        }
        else {
            $('#LocationModalErrorMessage').text(message);
            $('#LocationModalErrorMessage').show();
        }
    }


    /**************** EDIT LOCATION *******************/

    function ValidateEditLocation(newEditLocation, originalLocation, currentLocations) {
        ClearEditLocationErrors();
        var valid = true;

        if (newEditLocation.name != originalLocation.name) {
            if (newEditLocation.name.trim() == "") {
                valid = false;
                AddElementError("#locationNameCont", "Must specify a location name");
            } else {
                var nameValid = true;
                for (var i = 0; i < currentLocations.length; i++) {
                    if (newEditLocation.name.toLowerCase() === currentLocations[i].name.toLowerCase()) {
                        nameValid = false;
                        break;
                    }
                }
                if (!nameValid) {
                    valid = false;
                    AddElementError("#locationNameCont", "Location name already exists");
                }
            }
        }

        if ($("#facilitiesEmail").val().trim() != "") {
            if (!ValidateEmail($("#facilitiesEmail").val().trim())) {
                valid = false;
                AddElementError("#facilitiesDetailEmailCont", "Invalid email address for Facilities");
            }
        }
        if ($("#securityEmail").val().trim() != "") {
            if (!ValidateEmail($("#securityEmail").val().trim())) {
                valid = false;
                AddElementError("#securityDetailEmailCont", "Invalid email address for Security");
            }
        }
        if ($("#dssEmail").val().trim() != "") {
            if (!ValidateEmail($("#dssEmail").val().trim())) {
                valid = false;
                AddElementError("#dssDetailEmailCont", "Invalid email address for DSS");
            }
        }
        return valid;
    }

    function ClearEditLocationErrors() {
        $("#editLocationErrorCont").css('display', 'none');
        $("#editLocationErrorList").html('');

        $(".has-error").removeClass('has-error');
    }

    function ClearEditLocationModalFields() {
        $("#editLocationModal input").val('');
    }

    function AddElementError(elementSelector, errorMessage) {
        $(elementSelector).addClass('has-error');
        AddEditLocationErrorMessage(errorMessage);
    }


    function AddEditLocationErrorMessage(message) {
        var errorList = $("#editLocationErrorList");
        if (errorList.children('li').length == 0) {
            $("#editLocationErrorCont").css('display', 'block');
        }
        errorList.append('<li>' + message + '</li>');
    }

    $(function () {
        $("#editLocationModal").on("hidden.bs.modal", function () {
            ClearEditLocationErrors();
            ClearEditLocationModalFields();
            $("#editLocationModal #accordion").collapse('hide');
        });
    })

})();