import { AxiosPromise } from 'axios';
import { axiosInstance } from '@/shared/utils/axios-instance';
import { CriteriaDrivenBudgets } from '../shared/models/iAM/criteria-driven-budgets';
import {getAuthHeader} from '@/shared/utils/authentication-header';

export default class BudgetCriteriaService {
    /**
     * Gets BudgetCriteria data
     * @param selectedScenarioId Scenario object id
     */
    static getBudgetCriteria(selectedScenarioId: number): AxiosPromise {
        return axiosInstance.get(`/api/GetCriteriaDrivenBudgets/${selectedScenarioId}`, {headers: getAuthHeader()});
    }

    /**
     * Saves BudgetCriteria data
     * @param selectedScenarioId Scenario id
     * @param budgetCriteria List of BudgetCriteria objects
     */
    static saveBudgetCriteria(selectedScenarioId: number, budgetCriteria: CriteriaDrivenBudgets[]): AxiosPromise {
        return axiosInstance.post(`/api/SaveCriteriaDrivenBudgets/${selectedScenarioId}`, budgetCriteria, {headers: getAuthHeader()});
    }
}