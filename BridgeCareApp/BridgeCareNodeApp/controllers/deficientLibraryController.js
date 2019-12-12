function deficientLibraryController(DeficientLibrary) {
    /**
     * GET NodeJS API endpoint for deficient libraries; gets & returns all deficient libraries
     * @param request
     * @param response
     */
    function get(request, response) {
        DeficientLibrary.find((error, deficientLibraries) => {
            if (error)
                return response.status(500).json(error);

            return response.status(200).json(deficientLibraries);
        });
    }

    /**
     * POST NodeJS API endpoint for deficient libraries; creates & returns a deficient library
     * @param request Http request
     * @param response Http response
     */
    function post(request, response) {
        const deficientLibrary = new DeficientLibrary(request.body);

        if (!request.body.name) {
            return response.status(400).send('Library name is required');
        }

        deficientLibrary.save(function(error, library) {
            if (error)
                return response.status(500).json(error);

            return response.status(200).json(library);
        });
    }

    /**
     * PUT NodeJS API endpoint for deficient libraries; updates & returns a deficient library
     * @param request Http request
     * @param response Http response
     */
    // function put(request, response) {
    //     ownerRestriction = request.user.role === 'PD-BAMS-ADMINISTRATOR' ? {} : {owner: [request.user.username]};
    //     DeficientLibrary.findOneAndUpdate({_id: request.body._id, ...ownerRestriction}, request.body, {new: true}, (error, DeficientLibrary) => {
    //         if (error)
    //             return response.status(500).json(error);
    //         if (deficientLibrary)
    //             return response.status(200).json(DeficientLibrary);
    //         return response.status(500).json({error: `User ${request.user.username} has no deficient library with id ${request.params.libraryId}`})
    //     });
    // }
    const {getUpdateFunction, getDeletionFunction} = require('./libraryAPIFunctions');

    put = getUpdateFunction(DeficientLibrary);
    deleteLibrary = getDeletionFunction(DeficientLibrary);

    return {get, post, put, deleteLibrary};
}

module.exports = deficientLibraryController;
