describe('Locations Service', function () {

    var LocationsService;
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_LocationsService_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        LocationsService = _LocationsService_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(LocationsService).toBeDefined();
    });

    describe('query method', function () {

        it('should exist', function () {
            expect(LocationsService.query).not.toEqual(null);
            expect(LocationsService.query).toBeDefined();
        })

        it('should return an array of all locations', function () {

            var locations = locationMockData.getMockLocations();                        

            $httpBackend.expectGET('/api/locations')
                .respond(200, locations);

            var result = LocationsService.query();

            $httpBackend.flush();

            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(2);
        });
    });

    describe('getByID method', function () {

        it('should exist', function () {
            expect(LocationsService.getByID).not.toEqual(null);
            expect(LocationsService.getByID).toBeDefined();
        })

        it('should return a location object when passed an ID', function () {

            var location = locationMockData.getSingleMockLocation;

            $httpBackend.expectGET('/api/locations/' + location.id)
                .respond(200, location);

            var result = LocationsService.getByID({
                id: location.id
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result.name).toEqual(location.name);
        });
    });

    describe('getByStatus method', function () {

        it('should exist', function () {
            expect(LocationsService.getByStatus).not.toEqual(null);
            expect(LocationsService.getByStatus).toBeDefined();
        })

        it('should return a set of locations based on the parameters provided', function () {

            var locations = locationMockData.getMockLocations(),
                status = true,
                extraInfo = true;

            $httpBackend.expectGET('/api/locations/' + status + '/' + extraInfo)
                .respond(200, locations);

            var result = LocationsService.getByStatus({
                status: status,
                extraInfo: extraInfo
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(locations.length);
        });
    });

    describe('update method', function () {

        it('should exist', function () {
            expect(LocationsService.update).not.toEqual(null);
            expect(LocationsService.update).toBeDefined();
        });

        it('should update an existing location object', function () {

            var location = locationMockData.getSingleMockLocation;

            $httpBackend.expectPUT('/api/locations/' + location.id, location)
                .respond(200);

            LocationsService.update({
                id: location.id
            }, location);

            expect($httpBackend.flush).not.toThrow();
        });
    });

    describe('updateStatus method', function () {

        it('should exist', function () {
            expect(LocationsService.updateStatus).not.toEqual(null);
            expect(LocationsService.updateStatus).toBeDefined();
        });

        it('should update an existing location status', function () {

            var location = locationMockData.getSingleMockLocation,
                locationStatus = false;

            $httpBackend.expectPATCH('/api/locations/' + location.id + '/' + locationStatus)
                .respond(200);

            LocationsService.updateStatus({
                id: location.id,
                status: locationStatus
            });

            expect($httpBackend.flush).not.toThrow();

        });
    });
});