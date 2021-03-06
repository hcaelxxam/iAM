import {emptyRemainingLifeLimitLibrary, RemainingLifeLimitLibrary} from '@/shared/models/iAM/remaining-life-limit';
import {clone, any, propEq, findIndex, append, equals} from 'ramda';
import RemainingLifeLimitService from '@/services/remaining-life-limit.service';
import {AxiosResponse} from 'axios';
import {hasValue} from '@/shared/utils/has-value-util';
import {convertFromMongoToVue} from '@/shared/utils/mongo-model-conversion-utils';

const state = {
    remainingLifeLimitLibraries: [] as RemainingLifeLimitLibrary[],
    scenarioRemainingLifeLimitLibrary: clone(emptyRemainingLifeLimitLibrary) as RemainingLifeLimitLibrary,
    selectedRemainingLifeLimitLibrary: clone(emptyRemainingLifeLimitLibrary) as RemainingLifeLimitLibrary
};

const mutations = {
    /**
     * Sets state.remainingLifeLimitLibraries to a copy of remainingLifeLimitLibraries
     * @param state App state
     * @param remainingLifeLimitLibraries List of remaining life limit libraries
     */
    remainingLifeLimitLibrariesMutator(state: any, remainingLifeLimitLibraries: RemainingLifeLimitLibrary[]) {
        state.remainingLifeLimitLibraries = clone(remainingLifeLimitLibraries);
    },
    /**
     * Sets state.selectedRemainingLifeLimitLibrary with an existing remaining life limit library found in
     * state.remainingLifeLimitLibraries
     * @param state App state
     * @param remainingLifeLimitLibraryId Remaining life limit library id
     */
    selectedRemainingLifeLimitLibraryMutator(state: any, remainingLifeLimitLibraryId: string) {
        if (any(propEq('id', remainingLifeLimitLibraryId), state.remainingLifeLimitLibraries)) {
            state.selectedRemainingLifeLimitLibrary = clone(state.remainingLifeLimitLibraries
                .find((remainingLifeLimitLibrary: RemainingLifeLimitLibrary) =>
                    remainingLifeLimitLibrary.id === remainingLifeLimitLibraryId
                ) as RemainingLifeLimitLibrary
            );
        } else {
            state.selectedRemainingLifeLimitLibrary = clone(emptyRemainingLifeLimitLibrary);
        }
    },
    /**
     * Sets state.selectedRemainingLifeLimitLibrary with a copy of updatedSelectedRemainingLifeLimitLibrary
     * @param state App state
     * @param updatedSelectedRemainingLifeLimitLibrary Remaining life limit library
     */
    updatedSelectedRemainingLifeLimitLibraryMutator(state: any, updatedSelectedRemainingLifeLimitLibrary: RemainingLifeLimitLibrary) {
        state.selectedRemainingLifeLimitLibrary = clone(updatedSelectedRemainingLifeLimitLibrary);
    },
    /**
     * Appends createdRemainingLifeLimitLibrary to state.remainingLifeLimitLibraries
     * @param state App state
     * @param createdRemainingLifeLimitLibrary Created remaining life limit library
     */
    createdRemainingLifeLimitLibraryMutator(state: any, createdRemainingLifeLimitLibrary: RemainingLifeLimitLibrary) {
        state.remainingLifeLimitLibraries = append(createdRemainingLifeLimitLibrary, state.remainingLifeLimitLibraries);
    },
    /**
     * Updates a remaining life limit library in state.remainingLifeLimitLibraries with updatedRemainingLifeLimitLibrary
     * @param state App state
     * @param updatedRemainingLifeLimitLibrary Updated remaining life limit library
     */
    updatedRemainingLifeLimitLibraryMutator(state: any, updatedRemainingLifeLimitLibrary: RemainingLifeLimitLibrary) {
        if (any(propEq('id', updatedRemainingLifeLimitLibrary.id), state.remainingLifeLimitLibraries)) {
            const remainingLifeLimitLibraries: RemainingLifeLimitLibrary[] = clone(state.remainingLifeLimitLibraries);
            const index: number = findIndex(propEq('id', updatedRemainingLifeLimitLibrary.id), remainingLifeLimitLibraries);
            remainingLifeLimitLibraries[index] = updatedRemainingLifeLimitLibrary;
            state.remainingLifeLimitLibraries = remainingLifeLimitLibraries;
        }
    },
    /**
     * Sets state.scenarioRemainingLifeLimitLibrary with a copy of scenarioRemainingLifeLimitLibrary
     * @param state App state
     * @param scenarioRemainingLifeLimitLibrary Scenario's remaining life limit library
     */
    scenarioRemainingLifeLimitLibraryMutator(state: any, scenarioRemainingLifeLimitLibrary: RemainingLifeLimitLibrary) {
        state.scenarioRemainingLifeLimitLibrary = clone(scenarioRemainingLifeLimitLibrary);
    }
};

const actions = {
    selectRemainingLifeLimitLibrary({commit}: any, payload: any) {
        commit('selectedRemainingLifeLimitLibraryMutator', payload.remainingLifeLimitLibraryId);
    },
    updateSelectedRemainingLifeLimitLibrary({commit}: any, payload: any) {
        commit('updatedSelectedRemainingLifeLimitLibraryMutator', payload.updatedSelectedRemainingLifeLimitLibrary);
    },
    async getRemainingLifeLimitLibraries({commit}: any) {
        await RemainingLifeLimitService.getRemainingLifeLimitLibraries()
            .then((response: AxiosResponse<any[]>) => {
                if (hasValue(response, 'data')) {
                    const remainingLifeLimitLibraries: RemainingLifeLimitLibrary[] = response.data
                        .map((data: any) => convertFromMongoToVue(data));
                    commit('remainingLifeLimitLibrariesMutator', remainingLifeLimitLibraries);
                }
            });
    },
    async createRemainingLifeLimitLibrary({dispatch, commit}: any, payload: any) {
        await RemainingLifeLimitService.createRemainingLifeLimitLibrary(payload.createdRemainingLifeLimitLibrary)
            .then((response: AxiosResponse<any>) => {
                if (hasValue(response, 'data')) {
                    const createdRemainingLifeLimitLibrary: RemainingLifeLimitLibrary = convertFromMongoToVue(response.data);
                    commit('createdRemainingLifeLimitLibraryMutator', createdRemainingLifeLimitLibrary);
                    dispatch('setSuccessMessage', {message: 'Successfully created remaining life limit library'});
                }
            });
    },
    async updateRemainingLifeLimitLibrary({dispatch, commit}: any, payload: any) {
        await RemainingLifeLimitService.updateRemainingLifeLimitLibrary(payload.updatedRemainingLifeLimitLibrary)
            .then((response: AxiosResponse<any>) => {
                if (hasValue(response, 'data')) {
                    const updatedRemainingLifeLimitLibrary: RemainingLifeLimitLibrary = convertFromMongoToVue(response.data);
                    commit('updatedRemainingLifeLimitLibraryMutator', updatedRemainingLifeLimitLibrary);
                    commit('selectedRemainingLifeLimitLibraryMutator', updatedRemainingLifeLimitLibrary.id);
                    dispatch('setSuccessMessage', {message: 'Successfully updated remaining life limit library'});
                }
            });
    },
    async getScenarioRemainingLifeLimitLibrary({dispatch, commit}: any, payload: any) {
        if (payload.selectedScenarioId > 0) {
            await RemainingLifeLimitService.getScenarioRemainingLifeLimitLibrary(payload.selectedScenarioId)
                .then((response: AxiosResponse<any>) => {
                    if (hasValue(response, 'data')) {
                        commit('scenarioRemainingLifeLimitLibraryMutator', response.data);
                        commit('updatedSelectedRemainingLifeLimitLibraryMutator', response.data);
                    }
                });
        } else {
            commit('scenarioRemainingLifeLimitLibraryMutator', emptyRemainingLifeLimitLibrary);
            commit('updatedSelectedRemainingLifeLimitLibraryMutator', emptyRemainingLifeLimitLibrary);
        }
    },
    async saveScenarioRemainingLifeLimitLibrary({dispatch, commit}: any, payload: any) {
        await RemainingLifeLimitService.saveScenarioRemainingLifeLimitLibrary(payload.saveScenarioRemainingLifeLimitLibraryData)
            .then((response: AxiosResponse<any>) => {
                if (hasValue(response, 'data')) {
                    commit('scenarioRemainingLifeLimitLibraryMutator', response.data);
                    dispatch('setSuccessMessage', {message: 'Successfully saved scenario remaining life limit library'});
                }
            });
    },
    async socket_remainingLifeLimitLibrary({dispatch, state, commit}: any, payload: any) {
        if (hasValue(payload, 'operationType') && hasValue(payload, 'fullDocument')) {
            if (payload.operationType == 'update' || payload.operationType == 'replace') {
                const updatedRemainingLifeLimitLibrary: RemainingLifeLimitLibrary = convertFromMongoToVue(payload.fullDocument);
                commit('updatedRemainingLifeLimitLibraryMutator', updatedRemainingLifeLimitLibrary);
                if (state.selectedRemainingLifeLimitLibrary.id === updatedRemainingLifeLimitLibrary.id &&
                    !equals(state.selectedRemainingLifeLimitLibrary, updatedRemainingLifeLimitLibrary)) {
                    commit('selectedRemainingLifeLimitLibraryMutator', updatedRemainingLifeLimitLibrary.id);
                    dispatch('setInfoMessage', {message: 'Library data has been changed from another source'});
                }
            }

            if (payload.operationType === 'insert') {
                const createdRemainingLifeLimitLibrary: RemainingLifeLimitLibrary = convertFromMongoToVue(payload.fullDocument);
                commit('createdRemainingLifeLimitLibraryMutator', createdRemainingLifeLimitLibrary);
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
