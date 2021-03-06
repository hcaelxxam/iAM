import {AxiosPromise} from 'axios';
import {axiosInstance, nodejsAxiosInstance} from '@/shared/utils/axios-instance';
import {DeficientLibrary} from '@/shared/models/iAM/deficient';
import {convertFromVueToMongo} from '@/shared/utils/mongo-model-conversion-utils';

export default class DeficientService {
    /**
     * Gets deficient libraries
     */
    static getDeficientLibraries(): AxiosPromise {
        return nodejsAxiosInstance.get('/api/GetDeficientLibraries');
    }

    /**
     * Creates a deficient library
     * @param createdDeficientLibrary The deficient library create data
     */
    static createDeficientLibrary(createdDeficientLibrary: DeficientLibrary): AxiosPromise {
        return nodejsAxiosInstance.post('/api/CreateDeficientLibrary', convertFromVueToMongo(createdDeficientLibrary));
    }

    /**
     * Updates a deficient library
     * @param updatedDeficientLibrary The deficient library update data
     */
    static updateDeficientLibrary(updatedDeficientLibrary: DeficientLibrary): AxiosPromise {
        return nodejsAxiosInstance.put('/api/UpdateDeficientLibrary', convertFromVueToMongo(updatedDeficientLibrary));
    }

    /**
     * Gets scenario deficient library data
     * @param selectedScenarioId Scenario object id
     */
    static getScenarioDeficientLibrary(selectedScenarioId: number): AxiosPromise {
        return axiosInstance.get(`/api/GetScenarioDeficientLibrary/${selectedScenarioId}`);
    }

    /**
     * Saves scenario deficient library data
     * @param scenarioDeficientLibraryData Scenario deficient library data
     */
    static saveScenarioDeficientLibrary(scenarioDeficientLibraryData: DeficientLibrary): AxiosPromise {
        return axiosInstance.post('/api/SaveScenarioDeficientLibrary', scenarioDeficientLibraryData);
    }
}