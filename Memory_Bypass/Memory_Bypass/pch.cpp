// pch.cpp: 与预编译标头对应的源文件

#include "pch.h"

// 当使用预编译的头时，需要使用此源文件，编译才能成功。
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <TlHelp32.h>
/// <summary>
/// 根据进程ID得到进程窗口句柄
/// </summary>
/// <param name="nID">进程PID</param>
/// <returns>返回窗口句柄(十六进制)</returns>
HANDLE GetProcessHandle(int nID)
{
    return OpenProcess(PROCESS_ALL_ACCESS, FALSE, nID);
}

/// <summary>
/// 根据窗口名称得到进程ID
/// </summary>
/// <param name="procName">窗口名称</param>
/// <returns>返回进程PID(十进制)</returns>
DWORD GetProcId(const char* procName)
{
    DWORD procId = 0;
    HANDLE hSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnap != INVALID_HANDLE_VALUE)
    {
        PROCESSENTRY32 procEntry;
        procEntry.dwSize = sizeof(procEntry);
        if (Process32First(hSnap, &procEntry))
        {
            do
            {
                if (lstrcmpi(procEntry.szExeFile,procName) == 0) {
                    procId = procEntry.th32ProcessID;
                    break;
                }
            } while (Process32Next(hSnap, &procEntry));
        }
    }
    CloseHandle(hSnap);
    return procId;
}

/// <summary>
/// 通过进程ID和窗口名称得到基地址
/// </summary>
/// <param name="procId">进程ID</param>
/// <param name="modName">窗口名称</param>
/// <returns>返回基地址(十进制)</returns>
uintptr_t GetModuleBaseAddress(DWORD procId, const char* modName)
{
    uintptr_t modBaseAddr = 0;
    HANDLE hSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, procId);
    if (hSnap != INVALID_HANDLE_VALUE)
    {
        MODULEENTRY32 modEntry;
        modEntry.dwSize = sizeof(modEntry);
        if (Module32First(hSnap, &modEntry))
        {
            do
            {
                if (!_stricmp(modEntry.szModule, modName))
                {
                    modBaseAddr = (uintptr_t)modEntry.modBaseAddr;
                    break;
                }
            } while (Module32Next(hSnap, &modEntry));
        }
    }
    CloseHandle(hSnap);
    return modBaseAddr;
}

/// <summary>
/// 写入内存
/// </summary>
/// <param name="Handle">句柄</param>
/// <param name="Address">内存地址</param>
/// <param name="Buffer">内容</param>
/// <returns>是否写入成功</returns>
bool WriteMemory(HANDLE Handle, long long Address, char Buffer[1])
{
    return WriteProcessMemory(Handle, LPVOID(Address), Buffer, 1, 0);
}

