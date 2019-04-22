// Open3dmmBootstrapper.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <windows.h>
#include <vector>
#include <iostream>
#include <metahost.h>
#pragma comment(lib, "mscoree.lib")

unsigned char Reserved[4194304];

void hacky_stub() {
	auto address = (void*)0x00400000;
	auto result = FreeLibrary((HMODULE)address);
	auto result2 = UnmapViewOfFile(address);
	auto newAddr = LoadLibraryA("3dmovie.exe");
	ExitProcess(0);
}

int main()
{
	auto address = (void*)0x00400000;

	std::vector<unsigned char> const code =
	{
		0x68,                   // push
		0x00, 0x00, 0x40, 0x00, // 0x00400000 (HMODULE)
		0xB8,                   // mov eax,
		0x00, 0x00, 0x00, 0x00, // ptr to FreeLibrary
		0xFF, 0xD0,				// call eax

		0x68,                   // push
		0x00, 0x00, 0x40, 0x00, // 0x00400000 (HMODULE)
		0xB8,                   // mov eax,
		0x00, 0x00, 0x00, 0x00, // ptr to UnmapViewOfFile
		0xFF, 0xD0,				// call eax

		0x68,                   // push
		0x00, 0x00, 0x00, 0x00, // ptr to moduleName
		0xB8,                   // mov eax,
		0x00, 0x00, 0x00, 0x00, // ptr to LoadLibraryA
		0xFF, 0xD0,				// call eax


		0x6A, 0x00,             // push 0 
		0x6A, 0x00,             // push 0
		0x68,					// push
		0x90, 0xE4, 0x41, 0x00, // offset string L"Bootstrap" (041E490h)
		0x68,					// push
		0xA8, 0xE4, 0x41, 0x00, // offset string L"Open3dmm.Progra\x4000" (041E4A8h)
		0x68,					// push
		0xD0, 0xE4, 0x41, 0x00, // offset string L"Open3dmm.exe" (041E4D0h)
		0xB9,					// mov ecx, 
		0x00, 0x00, 0x00, 0x00,	// [runtimeHost]
		0x8B, 0x11,				// mov edx,dword ptr[ecx]
		0x8B, 0xC1,				// mov eax, ecx
		0x50,					// push eax
		0x8B, 0x4A, 0x2C,       // mov ecx,dword ptr[edx + 2Ch]
		0xFF, 0xD1,             // call ecx

		// TODO: Pass return value to ExitProcess

		0x50,                   // push eax
		0xB8,                   // mov eax,
		0x00, 0x00, 0x00, 0x00, // ptr to ExitProcess
		0xFF, 0xD0,				// call eax
	  //0xC2, 0x04, 0x00		// ret 4
	};

	const HMODULE kernel32 = GetModuleHandleA("kernel32.dll");

	LPCSTR str = "";
	SYSTEM_INFO system_info;
	GetSystemInfo(&system_info);
	auto const page_size = system_info.dwPageSize;
	auto const buffer = VirtualAlloc(nullptr, page_size, MEM_COMMIT, PAGE_READWRITE);

	void* str_dest = (void*)((int)buffer + code.size());
	*(int*)(code.begin()._Ptr + 6) = (int)GetProcAddress(kernel32, "FreeLibrary");
	*(int*)(code.begin()._Ptr + 18) = (int)GetProcAddress(kernel32, "UnmapViewOfFile");
	*(int*)(code.begin()._Ptr + 25) = (int)str_dest + 80;
	*(int*)(code.begin()._Ptr + 30) = (int)GetProcAddress(kernel32, "LoadLibraryA");
	*(int*)(code.begin()._Ptr + 41) = (int)str_dest + 60;
	*(int*)(code.begin()._Ptr + 46) = (int)str_dest + 26;
	*(int*)(code.begin()._Ptr + 51) = (int)str_dest;
	*(int*)(code.begin()._Ptr + 72) = (int)GetProcAddress(kernel32, "ExitProcess");

	std::memcpy(str_dest, L"Open3dmm.dll\0Open3dmm.Program\0Bootstrap", 80);

	//TODO: Detect 3dmm install directory

	std::memcpy((void*)((int)str_dest + 80), "C:\\microsoft kids\\3d movie maker\\3dmovie.exe", 45);

	// CLR

	ICLRMetaHost * metaHost = NULL;
	ICLRRuntimeInfo * runtimeInfo = NULL;
	ICLRRuntimeHost * runtimeHost = NULL;

	if (CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)& metaHost) == S_OK)
		if (metaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)& runtimeInfo) == S_OK)
			if (runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)& runtimeHost) == S_OK)
				if (runtimeHost->Start() == S_OK)
				{
					*(int*)(code.begin()._Ptr + 56) = (int)runtimeHost;
					std::memcpy(buffer, code.data(), code.size());
					DWORD dummy;
					VirtualProtect(buffer, code.size(), PAGE_EXECUTE_READ, &dummy);

					auto const hacky_function = reinterpret_cast<void(*)()>(buffer);
					hacky_function();
				}
}