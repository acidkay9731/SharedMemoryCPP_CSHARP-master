// SharedMemoryCPP.cpp : 콘솔 응용 프로그램에 대한 진입점을 정의합니다.
//

#include "stdafx.h"

#include <windows.h>
#include <stdio.h>
#include <conio.h>
#include <tchar.h>

#include <ctime>
#include <sstream>

#include <chrono>
#include <thread>
#include <stdlib.h>

using namespace std;

#define BUF_SIZE 256
TCHAR szName[] = TEXT("SharedMemory_CPP_CSHARP");
HANDLE hMapFile;
LPCTSTR pBuf;

void ServerWork();
void ClientWork();

//0:Server, 1:Client
int workMode = 0;

int _tmain(int argc, _TCHAR* argv[])
{
	printf("Input WorkMode [0]:Server/[1]Client: ");

#pragma warning(disable:4996)
	char input = getch();

	if (input == '0')
	{
		workMode = 0;
		printf("\r\n");
	}
	else if (input = '1')
	{
		workMode = 1;
		printf("\r\n");
	}
	else
	{
		printf("\r\nPlease Input 0 or 1.\r\n");
		return 0;
	}
	
	srand((unsigned int)time(NULL));

	if (workMode == 0)
	{

		hMapFile = CreateFileMapping(
			INVALID_HANDLE_VALUE,    // use paging file
			NULL,                    // default security
			PAGE_READWRITE,          // read/write access
			0,                       // maximum object size (high-order DWORD)
			BUF_SIZE,                // maximum object size (low-order DWORD)
			szName);                 // name of mapping object

		if (hMapFile == NULL)
		{
			_tprintf(TEXT("Could not create file mapping object (%d).\n"),
				GetLastError());
			return 1;
		}
		pBuf = (LPTSTR)MapViewOfFile(hMapFile,   // handle to map object
			FILE_MAP_ALL_ACCESS, // read/write permission
			0,
			0,
			BUF_SIZE);

		if (pBuf == NULL)
		{
			_tprintf(TEXT("Could not map view of file (%d).\n"),
				GetLastError());

			CloseHandle(hMapFile);

			return 1;
		}

		ServerWork();
	}
	else if (workMode == 1)
	{
		hMapFile = OpenFileMapping(
			FILE_MAP_ALL_ACCESS,   // read/write access
			FALSE,                 // do not inherit the name
			szName);               // name of mapping object

		if (hMapFile == NULL)
		{
			_tprintf(TEXT("Could not open file mapping object (%d).\n"),
				GetLastError());
			return 1;
		}

		pBuf = (LPTSTR)MapViewOfFile(hMapFile, // handle to map object
			FILE_MAP_ALL_ACCESS,  // read/write permission
			0,
			0,
			BUF_SIZE);

		if (pBuf == NULL)
		{
			_tprintf(TEXT("Could not map view of file (%d).\n"),
				GetLastError());

			CloseHandle(hMapFile);

			return 1;
		}

		ClientWork();
	}

	return 0;
}

void ServerWork()
{
	int nCount = 1000;

	while (nCount >= 0)
	{
		int nTemp = rand() % 100;

		//%02 To Need MemoryOverWrite
		printf("CountDown: %d, WriteMemory: %02d\r\n", nCount, nTemp);

		wchar_t msg[256];

		//%02 To Need MemoryOverWrite
		swprintf_s(msg, L"%02d", nTemp);

		const wchar_t *cMsg = const_cast<wchar_t*>(msg);

		CopyMemory((PVOID)pBuf, cMsg, (_tcslen(cMsg) * sizeof(TCHAR)));

		std::this_thread::sleep_for(std::chrono::milliseconds(1000 / 1));

		nCount--;
	}

	UnmapViewOfFile(pBuf);

	CloseHandle(hMapFile);
}

void ClientWork()
{
	int nCount = 1000;

	while (nCount >= 0)
	{
		char pstrDest[256];
		int nLen = (int)wcslen(pBuf);
		
#pragma warning(disable:4996)
		wcstombs(pstrDest, pBuf, nLen + 1);
		printf("CountDown: %d, ReadMemory: %s\r\n", nCount, pstrDest);

		std::this_thread::sleep_for(std::chrono::milliseconds(1000 / 1));

		nCount--;
	}

	UnmapViewOfFile(pBuf);

	CloseHandle(hMapFile);
}