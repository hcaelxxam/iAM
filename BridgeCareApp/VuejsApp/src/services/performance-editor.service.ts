import {AxiosPromise} from 'axios';
import {PerformanceLibrary, PerformanceLibraryEquation} from '@/shared/models/iAM/performance';
import {axiosInstance, nodejsAxiosInstance} from '@/shared/utils/axios-instance';
import {getAuthHeader} from '@/shared/utils/authentication-header';

const modifyDataForMongoDB = (performanceLibrary: PerformanceLibrary): any => {
    const performanceLibraryData: any = {
        ...performanceLibrary,
        _id: performanceLibrary.id,
        equations: performanceLibrary.equations.map((equation: PerformanceLibraryEquation) => {
            const equationData: any = {...equation, _id: equation.id};
            delete equationData.id;
            return equationData;
        })
    };
    delete performanceLibraryData.id;
    return performanceLibraryData;
};

export default class PerformanceEditorService {
    /**
     * Gets all performance Libraries a user can read/edit
     */
    static getPerformanceLibraries(): AxiosPromise {
        return nodejsAxiosInstance.get('/api/GetPerformanceLibraries');
    }

    /**
     * Creates a performance library
     * @param createPerformanceLibraryData The performance library create data
     */
    static createPerformanceLibrary(createPerformanceLibraryData: PerformanceLibrary): AxiosPromise {
        return nodejsAxiosInstance.post('/api/CreatePerformanceLibrary', modifyDataForMongoDB(createPerformanceLibraryData));
    }

    /**
     * Updates a performance library
     * @param updatePerformanceLibraryData The performance library update data
     */
    static updatePerformanceLibrary(updatePerformanceLibraryData: PerformanceLibrary): AxiosPromise {
        return nodejsAxiosInstance.put('/api/UpdatePerformanceLibrary', modifyDataForMongoDB(updatePerformanceLibraryData));
    }

    /**
     * Gets a scenario's performance library
     * @param selectedScenarioId Scenario object id
     */
    static getScenarioPerformanceLibrary(selectedScenarioId: number): AxiosPromise {
        return axiosInstance.get(`/api/GetScenarioPerformanceLibrary/${selectedScenarioId}`, {headers: getAuthHeader()});
    }

    /**
     * Saves a scenario performance library
     * @param saveScenarioPerformanceLibraryData The scenario performance library upsert data
     */
    static saveScenarioPerformanceLibrary(saveScenarioPerformanceLibraryData: PerformanceLibrary): AxiosPromise {
        return axiosInstance.post('/api/SaveScenarioPerformanceLibrary', saveScenarioPerformanceLibraryData, {headers: getAuthHeader()});
    }
}
