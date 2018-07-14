describe('Users Service', function () {

    var UsersService;
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_UsersService_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        UsersService = _UsersService_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(UsersService).toBeDefined();
    });

    describe('get method', function () {

        it('should exist', function () {
            expect(UsersService.get).not.toEqual(null);
            expect(UsersService.get).toBeDefined();
        })

        it('should return an array of matching users when passed a valid surname', function () {

            var users = usersMockData.getFilteredMockUsers();

            var singleUser = users[0];

            $httpBackend.expectGET('/api/users/' + singleUser.lastName)
                .respond(200, users);

            var result = UsersService.get({
                name: singleUser.lastName
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(1);
            expect(result[0].lastName).toEqual(singleUser.lastName);
        });
    });

});