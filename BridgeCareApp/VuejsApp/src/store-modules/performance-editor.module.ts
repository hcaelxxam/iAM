import {emptyPerformanceLibrary, PerformanceLibrary} from '@/shared/models/iAM/performance';
import {clone, append, any, propEq, findIndex, equals} from 'ramda';
import PerformanceEditorService from '@/services/performance-editor.service';
import {AxiosResponse} from 'axios';
import {hasValue} from '@/shared/utils/has-value-util';
import {convertFromMongoToVue} from '@/shared/utils/mongo-model-conversion-utils';

const state = {
    performanceLibraries: [] as PerformanceLibrary[],
    scenarioPerformanceLibrary: clone(emptyPerformanceLibrary) as PerformanceLibrary,
    selectedPerformanceLibrary: clone(emptyPerformanceLibrary) as PerformanceLibrary
};

const mutations = {
    performanceLibrariesMutator(state: any, performanceLibraries: PerformanceLibrary[]) {
        // update state.performanceLibraries with a clone of the incoming list of performance libraries
        state.performanceLibraries = clone(performanceLibraries);
    },
    selectedPerformanceLibraryMutator(state: any, performanceLibraryId: number) {
        if (any(propEq('id', performanceLibraryId), state.performanceLibraries)) {
            // find the existing performance library in state.performanceLibraries where the id matches performanceLibraryId,
            // clone it, then update state.selectedPerformanceLibrary with the cloned, existing performance library
            state.selectedPerformanceLibrary = clone(state.performanceLibraries
                .find((performanceLibrary: PerformanceLibrary) =>
                    performanceLibrary.id === performanceLibraryId
                ) as PerformanceLibrary);
        } else {
            // reset state.selectedPerformanceLibrary as an empty performance library
            state.selectedPerformanceLibrary = clone(emptyPerformanceLibrary);
        }
    },
    updatedSelectedPerformanceLibraryMutator(state: any, updatedSelectedPerformanceLibrary: PerformanceLibrary) {
        // update the state.selectedPerformanceLibrary with the updated selected performance library
        state.selectedPerformanceLibrary = clone(updatedSelectedPerformanceLibrary);
    },
    createdPerformanceLibraryMutator(state: any, createdPerformanceLibrary: PerformanceLibrary) {
        // append the created performance library to a cloned list of state.performanceLibraries, then update
        // state.performanceLibraries with the cloned list
        state.performanceLibraries = append(createdPerformanceLibrary, state.performanceLibraries);
    },
    updatedPerformanceLibraryMutator(state: any, updatedPerformanceLibrary: PerformanceLibrary) {
        if (any(propEq('id', updatedPerformanceLibrary.id), state.performanceLibraries)) {
            // clone the list of performance libraries in state
            const performanceLibraries: PerformanceLibrary[] = clone(state.performanceLibraries);
            // find the index of the existing performance library in the cloned list of performance libraries that has
            // a matching id with the updated performance library
            const index: number = findIndex(propEq('id', updatedPerformanceLibrary.id), performanceLibraries);
            // set the updated performance library at the specified index
            performanceLibraries[index] = clone(updatedPerformanceLibrary);
            // update state.performanceLibraries with the cloned list of performance libraries
            state.performanceLibraries = performanceLibraries;
        }
    },
    scenarioPerformanceLibraryMutator(state: any, scenarioPerformanceLibrary: PerformanceLibrary) {
        state.scenarioPerformanceLibrary = clone(scenarioPerformanceLibrary);
    }
};

const actions = {
    selectPerformanceLibrary({commit}: any, payload: any) {
        commit('selectedPerformanceLibraryMutator', payload.performanceLibraryId);
    },
    updateSelectedPerformanceLibrary({commit}: any, payload: any) {
        commit('updatedSelectedPerformanceLibraryMutator', payload.updatedSelectedPerformanceLibrary);
    },
    async getPerformanceLibraries({commit}: any) {
        await PerformanceEditorService.getPerformanceLibraries()
            .then((response: AxiosResponse<any[]>) => {
                if (hasValue(response, 'data')) {
                    const performanceLibraries: PerformanceLibrary[] = response.data
                        .map((data: any) => convertFromMongoToVue(data));
                    commit('performanceLibrariesMutator', performanceLibraries);
                }
            });
    },
    async createPerformanceLibrary({dispatch, commit}: any, payload: any) {
        await PerformanceEditorService.createPerformanceLibrary(payload.createdPerformanceLibrary)
            .then((response: AxiosResponse<any>) => {
                if (hasValue(response, 'data')) {
                    const createdPerformanceLibrary: PerformanceLibrary = convertFromMongoToVue(response.data);
                    commit('createdPerformanceLibraryMutator', createdPerformanceLibrary);
                    dispatch('setSuccessMessage', {message: 'Successfully created performance library'});
                }
            });
    },
    async updatePerformanceLibrary({dispatch, commit}: any, payload: any) {
        await PerformanceEditorService.updatePerformanceLibrary(payload.updatedPerformanceLibrary)
            .then((response: AxiosResponse<any>) => {
                if (hasValue(response, 'data')) {
                    const updatedPerformanceLibrary: PerformanceLibrary = convertFromMongoToVue(response.data);
                    commit('updatedPerformanceLibraryMutator', updatedPerformanceLibrary);
                    commit('selectedPerformanceLibraryMutator', updatedPerformanceLibrary.id);
                    dispatch('setSuccessMessage', {message: 'Successfully updated performance library'});
                }
            });
    },
    async getScenarioPerformanceLibrary({commit}: any, payload: any) {
        await PerformanceEditorService.getScenarioPerformanceLibrary(payload.selectedScenarioId)
            .then((response: AxiosResponse<PerformanceLibrary>) => {
                if (hasValue(response, 'data')) {
                    commit('scenarioPerformanceLibraryMutator', response.data);
                    commit('updatedSelectedPerformanceLibraryMutator', response.data);
                }
            });
    },
    async saveScenarioPerformanceLibrary({dispatch, commit}: any, payload: any) {
        await PerformanceEditorService.saveScenarioPerformanceLibrary(payload.saveScenarioPerformanceLibraryData)
            .then((response: AxiosResponse<PerformanceLibrary>) => {
                if (hasValue(response, 'data')) {
                    commit('scenarioPerformanceLibraryMutator', response.data);
                    dispatch('setSuccessMessage', {message: 'Successfully saved scenario performance library'});
                }
            });
    },
    async socket_performanceLibrary({dispatch, state, commit}: any, payload: any) {
        if (hasValue(payload, 'operationType') && hasValue(payload, 'fullDocument')) {
            if (payload.operationType == 'update' || payload.operationType == 'replace') {
                const updatedPerformanceLibrary: PerformanceLibrary = convertFromMongoToVue(payload.fullDocument);
                commit('updatedPerformanceLibraryMutator', updatedPerformanceLibrary);
                if (state.selectedPerformanceLibrary.id === updatedPerformanceLibrary.id &&
                    !equals(state.selectedPerformanceLibrary, updatedPerformanceLibrary)) {
                    commit('selectedPerformanceLibraryMutator', updatedPerformanceLibrary.id);
                    dispatch('setInfoMessage', {message: 'Library data has been changed from another source'});
                }
            }

            if (payload.operationType == 'insert') {
                const createdPerformanceLibrary: PerformanceLibrary = convertFromMongoToVue(payload.fullDocument);
                commit('createdPerformanceLibraryMutator', createdPerformanceLibrary);
            }
        }
    }
};

const getters = {};

export default {
    state,
    getters,
    actions,
    mutations
};
