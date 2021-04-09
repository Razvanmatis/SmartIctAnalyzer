#include <iostream>
#include <iomanip>
#include <thread>
#include <cstring>
#include <cstdio>
#include <cstdint>
#include <string>
#include <memory>
#include "Util.h"
#include "grpc/grpc.h"
#include "grpcpp/channel.h"
#include "grpcpp/client_context.h"
#include "grpcpp/create_channel.h"
#include "grpcpp/security/credentials.h"
#include "svfp_dll_api.h"

const int TARGET_A = 0;
const int TARGET_B = 1;
const uint32_t ID_CODE_ZED_BOARD = 0x23727093;
const uint32_t IR_LENGTH_ZED_BOARD = 6; // bits
const uint32_t SCAN_CHAIN_LENGTH_ZED_BOARD = 1077; // bits


uint8_t power_up(void* target);
uint8_t power_down(void* target);
uint8_t run_single_prog(const uint8_t enableA, const uint8_t enableB, const uint8_t LOOP_CNT);
uint8_t run_svf_sequence(void* target, const char* file_path, const char* log_append);


int main() {

    const uint8_t RUN_CNT = 3;
    // A, B, loop count
    if (run_single_prog(1, 0, RUN_CNT))    {
        std::cout << "run_single_prog() failed at some point." << std::endl;
        return 1;
    }
    return 0;
}


uint8_t power_up(void* target) {
    uint8_t ret = 0;
    uint32_t voltage_mv = 0;
    ret += SvfPlayerDll_SetVsupSwitch(target, 1, 0);
    ret += SvfPlayerDll_SetVmodSwitch(target, 1);
    ret += SvfPlayerDll_SetProgSwitches(target, 0x3ff);
    ret += SvfPlayerDll_ReadVoltage(target, 0, &voltage_mv);
    ret += SvfPlayerDll_ReadVoltage(target, 1, &voltage_mv);
    return ret;
}

uint8_t power_down(void* target) {
    uint8_t ret = 0;
    ret += SvfPlayerDll_SetVsupSwitch(target, 0, 100);
    ret += SvfPlayerDll_SetVmodSwitch(target, 0);
    ret += SvfPlayerDll_SetProgSwitches(target, 0);
    return ret;
}

uint8_t run_svf_sequence(void* target, const char* file_path, const char* log_path) {


    // single target with IR = 6, SCAN_CHAIN = 1077
    // frequency dont' care, fixed to 10 MHz, APE channel 0
    // NOK?
    int32_t err = 0;
    // align to byte
    std::array < uint8_t, SCAN_CHAIN_LENGTH_ZED_BOARD / 8 + 1 > scan_chain_resp;
    uint32_t id_code_resp;
    if ((err = SvfPlayerDll_InitializeSvfPlayer(target, IR_LENGTH_ZED_BOARD, SCAN_CHAIN_LENGTH_ZED_BOARD, &scan_chain_resp[0], &id_code_resp)) != 0) {
        std::cout << "error: SVF Player init failed, error code " << std::to_string(err) << std::endl;
        return 1;
    };

    if (id_code_resp != ID_CODE_ZED_BOARD) {
        std::cout << "SVF Player init failed: ID_CODE mismatch. Target connected??? Read: " <<
                  Util::to_hex_string(id_code_resp) << std::endl;
        return 1;
    }
    std::cout << "SVF Player init: ID_CODE Read: " <<
              Util::to_hex_string(id_code_resp) << std::endl;


    std::cout << "Read scan chain vector:" << std::endl;
    uint32_t i = 1, j = 1;
    const char* hex = "0123456789ABCDEF";
    std::cout << "0:  ";
    for (auto& byte : scan_chain_resp) {
        std::cout << "0x" << hex[byte >> 4 & 0xF] << hex[byte & 0xF] << " ";
        if ((i % 8) == 0) {
            std::cout << std::endl;
            std::string space = (j > 9) ? ": " : ":  ";
            std::cout << std::to_string(j) <<  space;
            j++;
        }
        i++;
    }
    std::cout << std::endl;

    // NOK?
    if ((err = SvfPlayerDll_ExecuteSvfSequence(target, file_path, log_path)) != 0) {
        std::cout << "error: SVF Player sequence exe failed, error code " << std::to_string(err) << std::endl;
        return 1;
    };
    Util::delay_ms(500);
    return 0;
}

uint8_t run_single_prog(const uint8_t enableA, const uint8_t enableB, const uint8_t LOOP_CNT) {
    void* prog = SvfPlayerDll_ConnectProgrammer("192.168.3.11", 15504);
    if (prog == NULL) {
        std::cout << "Error connectiing to programmer " << std::endl;
        return 1;
    }

    SvfPlayerDll_SetLoggingFunc(prog, [](const char* msg) -> void { std::cout << msg << std::endl; });

    void* targetA = nullptr;
    if (enableA) {
        targetA = SvfPlayerDll_CreateTarget(prog, TARGET_A);
    }

    void* targetB = nullptr;
    if (enableB) {
        targetB = SvfPlayerDll_CreateTarget(prog, TARGET_B);
    }
    if ((targetA == nullptr) && (enableA)) {
        std::cout << "Error target A handler creation " << std::endl;
        return 1;
    }
    if ((targetB == nullptr) && (enableB)) {
        std::cout << "Error target B handler creation " << std::endl;
        return 1;
    }

    if (SvfPlayerDll_InitProgrammer(prog, 12000, 3300)) {
        std::cout << "Error programmer init" << std::endl;
        return 1;
    }

    // start svfp service on msp
    if (SvfPlayerDll_Start(prog)) {
        std::cout << "Error starting SVFP service " << std::endl;
        return 1;
    }
    uint8_t error_event = 0;
    std::thread targetA_thread([targetA, LOOP_CNT, enableA, &error_event]() {
        if (!enableA) {
            return;
        }
        uint16_t fails = 0;
        for (int j = 0; j < LOOP_CNT; j++) {
            char tag[] = "svfp_log_data_A_";
            char app[10];
            sprintf(app, "%d", j);
            if (power_up(targetA)) {
                fails++;
                std::cout << "power up targetA failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            if (run_svf_sequence(targetA, "LedTest.svf", strcat(tag, app))) {
                fails++;
                std::cout << "SVFP sequence targetA failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            if (power_down(targetA)) {
                fails++;
                std::cout << "power up targetA failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            std::cout << "TargetA loop " << std::to_string(j + 1) << "/" << std::to_string(LOOP_CNT) <<
                      " success" << std::endl;
            std::cout << " " << std::endl;
        }
        error_event = fails;
    });


    std::thread targetB_thread([targetB, LOOP_CNT, enableB, &error_event]() {
        if (!enableB) {
            return;
        }
        uint16_t fails = 0;
        for (int j = 0; j < LOOP_CNT; j++) {
            char tag[] = "svfp_log_data_B_";
            char app[10];
            sprintf(app, "%d", j);
            if (power_up(targetB)) {
                fails++;
                std::cout << "power up targetB failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            if (run_svf_sequence(targetB, "LedTest.svf", strcat(tag, app))) {
                fails++;
                std::cout << "SVFP sequence targetB failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            if (power_down(targetB)) {
                fails++;
                std::cout << "power up targetB failed, cycle: " << std::to_string(j + 1) << ", fails " <<
                          std::to_string(fails) << " of " << std::to_string(LOOP_CNT) << std::endl;
                continue;
            }
            std::cout << "TargetB loop " << std::to_string(j + 1) << "/" << std::to_string(LOOP_CNT) <<
                      " success" << std::endl;
        }
        error_event = fails;
    });

    targetA_thread.join();
    targetB_thread.join();

// stop svfp service on msp
    if (SvfPlayerDll_Stop(prog)) {
        std::cout << "Error stopping SVFP service " << std::endl;
        return 1;
    }
    std::cout << "SVFP successfully executed - END" << std::endl;
    if (SvfPlayerDll_Cleanup(prog)) {
        std::cout << "Error stopping SVFP service " << std::endl;
        return 1;
    }
    return error_event;
}