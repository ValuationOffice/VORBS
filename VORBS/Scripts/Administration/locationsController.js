administration.controller('LocationsController', ['$scope', '$http', '$resource', LocationsController]);

function LocationsController($scope, $http, $resource) {

    CreateLocationAdminServices($resource);

    $scope.Locations = Locations.query({});

    $scope.GetLocationsByStatus = function () {
        if ($scope.selectedItem.type === "all") {
            $scope.Locations = Locations.query({});
        }
        if ($scope.selectedItem.type === "enabled") {
            $scope.Locations = Locations.query({ status: true });
        }
        if ($scope.selectedItem.type === "disabled") {
            $scope.Locations = Locations.query({ status: false });
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

    $($('#newLocationModal').on('show.bs.modal', function () {
        SetLocationModalErrorMessage("");
    }))

    $('#newLocationModal').on('hidden.bs.modal', function () {             
        $scope.newLocation.Name = '',        
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
                $.ajax({
                    type: "POST",
                    data: JSON.stringify($scope.newLocation),
                    url: "api/locations",
                    contentType: "application/json",
                    success: function (data, status) {
                        alert('New Location Added.');
                        ReloadThisPage("locations");
                    },
                    error: function (error) {
                        alert('Unable to create new Location.');
                        $('#confirmLocationBtn').prop('disabled', '');
                        $('#confirmLocationBtn').html('Confirm Location');
                    }
                });
            }            
            $('#confirmLocationBtn').prop('disabled', '');
            $('#confirmLocationBtn').html('Confirm Location');

        } catch (e) {
            $('#confirmLocationBtn').prop('disabled', '');
            $('#confirmLocationBtn').html('Confirm Location');
        }
    }
}

function CreateLocationAdminServices($resource) {

    Locations = $resource('/api/locations/:status', {}, {
        query: { method: 'GET', isArray: true }
    });

}

function ValidateNewLocation(newLocationName, locations, emails) {

    var valid = true;

    // Validate Location Name
    if (newLocationName === undefined || newLocationName === '') {
        SetLocationModalErrorMessage("Invalid Location Name.");
        valid = false;
    }
    else {
        // Check if Location Name already exists
        for (var i = 0; i < locations.length; i++) {
            if (newLocationName.toLowerCase() === locations[i].toLowerCase()) {
                SetLocationModalErrorMessage("Location Name already exists!");
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
            SetLocationModalErrorMessage("Invalid Email Detected: " + emails[i]);
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

